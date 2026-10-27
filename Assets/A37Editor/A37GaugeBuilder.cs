using System;
using System.Linq;
using System.Xml.Linq;
using System.Globalization;
using UnityEngine;

// Preserve source gauge pivots/calibration. Motion is supplied from native game
// telemetry by the runtime helper; these markers contain no test controls.
public static class A37GaugeBuilder {
 static string F(float v)=>v.ToString("R",CultureInfo.InvariantCulture);
 static float N(XElement e,string name,float value=0)=>e?.Element(name)==null?value:float.Parse(e.Element(name).Value,CultureInfo.InvariantCulture);
 static string Key(string p){
  if(p.Contains("airspeed-indicator/indicated-speed-kt"))return "speed";
  if(p.Contains("altimeter/indicated-altitude-ft"))return "altitude";
  if(p=="orientation/pitch-deg")return "pitch";
  if(p=="orientation/roll-deg")return "roll";
  if(p.StartsWith("orientation/heading"))return "heading";
  if(p.Contains("vertical-speed-indicator")&&p.EndsWith("fpm"))return "vsi";
  if(p.Contains("vertical-speed")&&p.EndsWith("fps"))return "vsifps";
  if(p.Contains("consumables/fuel/tank")&&p.EndsWith("level-norm"))return "fuel";
  if(p.Contains("accelerometer")&&p.Contains("indicated-g"))return "gforce";
  if(p=="sim/time/utc/day-seconds")return "clock";
  return null;
 }
 public static void Apply(XElement document,Transform group){
  foreach(var a in document.Elements("animation").Where(a=>a.Element("type")?.Value=="rotate")){
   string key=Key((a.Element("property")?.Value??"").TrimStart('/'));if(key==null)continue;
   var axis=a.Element("axis");if(axis==null)continue;
   Vector3 direction=A37VisualBuilder.FG(N(axis,"x"),N(axis,"y"),N(axis,"z"));if(direction.sqrMagnitude<.01f)continue;
   var c=a.Element("center");Vector3 center=A37VisualBuilder.FG(N(c,"x-m"),N(c,"y-m"),N(c,"z-m"));
   foreach(var name in a.Elements("object-name").Select(e=>e.Value)){
    var target=group.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name==name);if(!target)continue;
    var pivot=new GameObject("A37GaugeAxis").transform;pivot.SetParent(target.parent,false);pivot.position=group.TransformPoint(center);pivot.rotation=group.rotation*Quaternion.FromToRotation(Vector3.right,direction.normalized);
    var motion=new GameObject("A37Gauge_"+key+"_"+F(N(a,"factor",1))+"_"+F(N(a,"offset-deg"))+"_"+F(N(a,"min-deg",-1e9f))+"_"+F(N(a,"max-deg",1e9f))).transform;motion.SetParent(pivot,false);target.SetParent(motion,true);
    var curve=a.Element("interpolation");if(curve!=null)foreach(var e in curve.Elements("entry")){var p=new GameObject("A37Calibration_"+F(N(e,"ind"))+"_"+F(N(e,"dep"))).transform;p.SetParent(motion,false);}
   }
  }
 }
}
