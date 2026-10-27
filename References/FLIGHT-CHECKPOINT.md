# Flight checkpoint - 2026-10-27 01:02 Uruguay

Build 12 PASSED native takeoff and climb >100 m at 03:48:10Z; all airframe parts
remained at 100 percent through more than eight minutes of controlled flight.
Gear retracted and extended, native orbit camera entered with user's actual
current Switch View key L. Two decorative crew and Artigas fin flashes visible.
Number stencils have surface clipping; increased surface offset is pending.

Clean configuration: 45 percent fuel at spawn, mass about 3508 kg, no external
weapons. Two independent native dry engines, no afterburner. Sustained speed
about 457 km/h at 1630 m altitude is TOO LOW for release. Need verify actual
Airbrake.openAmount and cached controlInputs.throttle before aerodynamic tuning.
HUD shows AIRBRAKE even though test engine inputs are full power; could be a
fixture input reference issue. Probe now logs both actual fields.

Runway fixture root cause verified: old placement left the casts below actual
pavement and disturbed local hinge poses. Fixed by measuring actual layer-6
pavement, including all owned rigidbodies, restoring local hinge pose, then
allowing native suspension forces. All 3 wheels remain enabled/in contact,
all parts 100 after placement with original 0.22 m suspension. Experimental
0.50 m suspension / dedicated casts were REVERTED, because unnecessary.

Landing test from far away failed before relocation: native GetTakeoffRunway
returned no runway. No landing acceptance pass. Fixture now retains the known
runway from initial placement and retains test control inputs on command error.

Build 15 installed/in next test: original proven gear and mesh-derived fixed
wing lift stations. Native trainer swept-wing force points were far aft of
A-37 geometry; updated from actual 40 percent chord sections at each station.
This new aerodynamic fit still needs fresh-spawn takeoff/landing validation.

Build 16 compiles while 15 is tested: custom 7.62 mm minigun using native firing,
rearming and muzzle effects, five loadout sets (gun plus four paired pylons).
Not yet installed or tested; do not publish as playable yet.
