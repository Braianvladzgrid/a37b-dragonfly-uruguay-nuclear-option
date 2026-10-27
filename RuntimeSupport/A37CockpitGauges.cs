using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

// A small telemetry bridge for the source's original analog needles. It does
// not simulate an authentic A-37 avionics/electrical system or modify flight.
public sealed class A37CockpitGauges:MonoBehaviour {
 sealed class Gauge{public Transform transform;public string key;public float factor,offset,min,max;public Vector2[] points;}
 Aircraft aircraft;float retry;readonly List<Gauge> gauges=new List<Gauge>();
 static float F(string value)=>float.Parse(value,CultureInfo.InvariantCulture);
 void Awake(){aircraft=GetComponent<Aircraft>();}
 void LateUpdate(){
  if(!aircraft)return;
  if(gauges.Count==0){if(Time.unscaledTime<retry)return;retry=Time.unscaledTime+1;foreach(var t in aircraft.GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("A37Gauge_"))){var a=t.name.Split('_');var points=Enumerable.Range(0,t.childCount).Select(t.GetChild).Where(c=>c.name.StartsWith("A37Calibration_")).Select(c=>{var p=c.name.Split('_');return new Vector2(F(p[1]),F(p[2]));}).OrderBy(p=>p.x).ToArray();gauges.Add(new Gauge{transform=t,key=a[1],factor=F(a[2]),offset=F(a[3]),min=F(a[4]),max=F(a[5]),points=points});}if(gauges.Count>0)Debug.Log("[A37Gauges] Bound "+gauges.Count+" calibrated source movements.");}
  foreach(var g in gauges)if(g.transform){float value=Value(g.key);if(g.points.Length>0){if(value<=g.points[0].x)value=g.points[0].y;else if(value>=g.points[g.points.Length-1].x)value=g.points[g.points.Length-1].y;else for(int i=1;i<g.points.Length;i++)if(value<=g.points[i].x){value=Mathf.Lerp(g.points[i-1].y,g.points[i].y,Mathf.InverseLerp(g.points[i-1].x,g.points[i].x,value));break;}}g.transform.localRotation=Quaternion.AngleAxis(-Mathf.Clamp(value*g.factor+g.offset,g.min,g.max),Vector3.right);}
 }
 float Value(string key){switch(key){case "speed":return aircraft.speed*1.9438445f;case "altitude":return aircraft.GlobalPosition().y*3.2808399f;case "pitch":return -Mathf.DeltaAngle(0,aircraft.transform.eulerAngles.x);case "roll":return -Mathf.DeltaAngle(0,aircraft.transform.eulerAngles.z);case "heading":return aircraft.transform.eulerAngles.y;case "vsi":return aircraft.CockpitRB()?aircraft.CockpitRB().velocity.y*196.8504f:0;case "vsifps":return aircraft.CockpitRB()?aircraft.CockpitRB().velocity.y*3.2808399f:0;case "gforce":return aircraft.gForce;case "clock":return(float)DateTime.UtcNow.TimeOfDay.TotalSeconds;case "fuel":var tanks=aircraft.GetFuelTanks();if(tanks==null)return 0;float capacity=tanks.Sum(t=>t?t.GetCapacity():0);return capacity>0?Mathf.Clamp01(tanks.Sum(t=>t?t.fuelMass:0)/capacity):0;default:return 0;}}
}
