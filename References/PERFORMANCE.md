# A-37B performance targets

The user requested handling and statistics close to the real Uruguayan A-37B.
Configuration targets must be distinguished from measurements in Nuclear Option.

Primary Uruguayan sources:

- FAU Museo Aeronautico: https://museo.fau.mil.uy/aeronave/fichas/cessna_a-37_b
  Two J85-GE-17A engines, 1,293 kgf each; maximum speed 770 km/h; external bombs
  or rockets up to 1,800 kg; two side-by-side crew; retractable tricycle gear.
- FAU 2019 yearbook, page 88:
  https://www.fau.mil.uy/uploads/archivos/file_b63f73301f.pdf
  Confirms eight underwing supports, 770 km/h, two 2,850 lbf engines and gives
  maximum takeoff mass 7,000 kg. Its minigun rate differs from the museum page;
  do not silently present either rate as an authenticated aircraft manual.
- Chilean national aviation museum:
  https://museoaeronautico.dgac.gob.cl/2015/07/21/15-a-40-anos-del-arribo-del-cessna-a-37-a-la-fach/
  A-37 data: empty mass 2,817 kg, span 10.93 m, length 8.62 m, height 2.7 m.
  Its published maximum mass and speed differ from the FAU figures. FAU figures
  take priority for the Uruguayan adaptation; these are not all equivalent
  aircraft configurations or test conditions.

Integration targets: 25,360 N total dry thrust, 2,817 kg empty mass, 1,536 kg
internal/tip fuel (1,920 litres at an assumed 0.8 kg/l), approximately 17.09 m2
wing area, no afterburner or thrust vectoring. Fuel and wing area are engineering
inputs based on the pinned upstream source, not authenticated FAU manual values.
Catalog/AI target speed is 770 km/h; no artificial velocity clamp is used.

Approach/takeoff speeds are provisional tuning inputs. A source comment mixes
98 knots with 98 km/h, so its stall-speed wording must not be copied literally.
Flight testing must record loadout, fuel, altitude, measured speed and health.

Minigun: 7.62 mm and 1500 rpm follow the FAU museum page. The FAU yearbook gives a different cyclic rate, so this is a documented source choice. 1500 rounds, 850 m/s, cartridge mass 24 g, 8.1 N s recoil and native damage parameters are provisional game integration inputs, not certified FAU weapon-manual data.

Engine integration uses native Turbojet.maxSpeed=205m/s as the onset of thrust
falloff, rather than a velocity clamp. This is an engineering fit to the FAU
speed target, not a published J85 compressor/thrust curve. Build18's previous
260m/s onset let clean speed exceed850km/h around1720m. Build19 is unmeasured.
