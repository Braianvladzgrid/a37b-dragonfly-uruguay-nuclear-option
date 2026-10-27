using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using HarmonyLib;
using UnityEngine;

// Visual adaptation and explicit body mass stations only. Native forces,
// suspension, braking, control inputs and damage remain active.
[BepInPlugin("vladzgrid.a37.flightsupport","A-37B Flight Support","0.1.0")]
public sealed class A37FlightSupport : BaseUnityPlugin {
 float nextScan;
 public static bool IsA37(Aircraft a)=>a&&a.definition&&a.definition.jsonKey=="A37B_Dragonfly_Uruguay";
 void Awake(){new Harmony("vladzgrid.a37.flightsupport").PatchAll(typeof(A37FlightSupport));}
 void Update(){if(Time.unscaledTime<nextScan)return;nextScan=Time.unscaledTime+.5f;foreach(var a in FindObjectsOfType<Aircraft>(true))if(IsA37(a)&&a.partLookup!=null){if(!a.GetComponent<A37SourceAnimations>())a.gameObject.AddComponent<A37SourceAnimations>();if(!a.GetComponent<A37CockpitGauges>())a.gameObject.AddComponent<A37CockpitGauges>();}}
 [HarmonyPatch(typeof(Aircraft),"GetInputs"),HarmonyPostfix]
 static void ManualBrake(Aircraft __instance,ControlInputs __result){if(!IsA37(__instance)||__result==null||!Rewired.ReInput.isReady)return;Aircraft local;if(GameManager.GetLocalAircraft(out local)&&local==__instance)__result.brake=Rewired.ReInput.players.GetPlayer(0).GetButton("Brake")?1:0;}
 [HarmonyPatch(typeof(LandingGear),"FixedUpdate"),HarmonyPrefix]
 static void WheelBrake(Aircraft ___aircraft,ControlInputs ___controlInputs){ManualBrake(___aircraft,___controlInputs);}
 static float AutomaticBrakeThreshold(LandingGear gear){var a=AccessTools.Field(typeof(LandingGear),"aircraft").GetValue(gear) as Aircraft;Aircraft local;if(!a||!GameManager.GetLocalAircraft(out local)||local!=a)return 1;var key=a.definition?a.definition.jsonKey:"";bool existing=key=="F5E_TigerII"&&AccessTools.TypeByName("F5FlightFix")!=null||key=="Su25_Frogfoot"&&AccessTools.TypeByName("Su25FlightFix")!=null;return IsA37(a)||existing?-1:1;}
 [HarmonyPatch(typeof(LandingGear),"FixedUpdate"),HarmonyTranspiler,HarmonyPriority(Priority.Last),HarmonyAfter("vladzgrid.f5.flightsupport","vladzgrid.su25.flightfix")]
 static IEnumerable<CodeInstruction> AutomaticWheelBrake(IEnumerable<CodeInstruction> original){var code=original.ToList();int replaced=0;for(int i=0;i<code.Count;i++)if(code[i].opcode==OpCodes.Call&&code[i].operand is MethodInfo method&&method.Name=="AutomaticBrakeThreshold"&&(method.DeclaringType.Name=="F5FlightFix"||method.DeclaringType.Name=="Su25FlightFix")){code[i].operand=AccessTools.Method(typeof(A37FlightSupport),"AutomaticBrakeThreshold");replaced++;}for(int i=1;i<code.Count;i++)if(code[i].opcode==OpCodes.Ldc_R4&&Convert.ToSingle(code[i].operand)==1&&code[i-1].opcode==OpCodes.Ldfld&&code[i-1].operand is FieldInfo field&&field.Name=="speed"&&field.DeclaringType==typeof(Unit)){var load=new CodeInstruction(OpCodes.Ldarg_0);load.labels.AddRange(code[i].labels);load.blocks.AddRange(code[i].blocks);code[i]=load;code.Insert(++i,new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(A37FlightSupport),"AutomaticBrakeThreshold")));replaced++;}if(replaced!=1)throw new InvalidOperationException("A37 automatic-brake adaptation expected one threshold; game or another patch changed.");return code;}
 [HarmonyPatch(typeof(Aircraft),"SetComplexPhysics"),HarmonyPostfix]
 static void MassStations(Aircraft __instance){if(!IsA37(__instance))return;var seen=new HashSet<Rigidbody>();foreach(var p in __instance.partLookup.OfType<AeroPart>())if(p.rb&&seen.Add(p.rb))p.rb.centerOfMass=p.rb.transform.InverseTransformPoint(p.transform.position);Debug.Log("[A37Physics] Native complex bodies initialized with explicit mass stations.");}
 [HarmonyPatch(typeof(PylonIndicator),"UpdateImages"),HarmonyPrefix]
 static void ExpandPylons(PylonIndicator __instance){var a=AccessTools.Field(typeof(PylonIndicator),"attachedAircraft").GetValue(__instance) as Aircraft;if(!IsA37(a))return;var f=AccessTools.Field(typeof(PylonIndicator),"pylonElementSets");var old=f.GetValue(__instance) as Array;if(old==null||old.Length>=8)return;var type=old.GetType().GetElementType();var expanded=Array.CreateInstance(type,8);Array.Copy(old,expanded,old.Length);for(int i=old.Length;i<8;i++){var item=Activator.CreateInstance(type);foreach(var field in type.GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic))if(field.FieldType.IsArray)field.SetValue(item,Array.CreateInstance(field.FieldType.GetElementType(),0));expanded.SetValue(item,i);}f.SetValue(__instance,expanded);}
}
public sealed class A37SourceAnimations:MonoBehaviour {
 static readonly FieldInfo Owner=AccessTools.Field(typeof(LandingGear),"aircraft"),Fold=AccessTools.Field(typeof(LandingGear),"foldAmount");
 Aircraft aircraft;LandingGear[] gear;readonly List<Track> tracks=new List<Track>();float retry;bool bound;
 sealed class Track {public Transform transform;public int channel;public float up,down;}
 void Awake(){aircraft=GetComponent<Aircraft>();}
 void LateUpdate(){
  if(!aircraft||aircraft.partLookup==null)return;
  if(!bound){if(Time.unscaledTime<retry)return;retry=Time.unscaledTime+.5f;var owned=aircraft.partLookup.OfType<AeroPart>().SelectMany(p=>p.GetComponentsInChildren<LandingGear>(true)).Concat(FindObjectsOfType<LandingGear>(true).Where(g=>Owner.GetValue(g) as Aircraft==aircraft)).Distinct().ToArray();
   if(owned.Length!=3)return;gear=new[]{owned.First(g=>g.name.Contains("_F")),owned.First(g=>g.name.Contains("_L")),owned.First(g=>g.name.Contains("_R"))};
   foreach(var t in aircraft.GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("A37GearMotion_"))){var fields=t.name.Split('_');tracks.Add(new Track{transform=t,channel=int.Parse(fields[1]),up=float.Parse(fields[2],CultureInfo.InvariantCulture),down=float.Parse(fields[3],CultureInfo.InvariantCulture)});}bound=tracks.Count==9;Debug.Log("[A37Gear] Source tracks="+tracks.Count+" native gears="+gear.Length);if(!bound)tracks.Clear();
  }
  if(!bound)return;
  foreach(var track in tracks)if(track.transform&&gear[track.channel])track.transform.localRotation=Quaternion.AngleAxis(Mathf.Lerp(track.down,track.up,Mathf.Clamp01((float)Fold.GetValue(gear[track.channel]))),Vector3.right);
 }
}



