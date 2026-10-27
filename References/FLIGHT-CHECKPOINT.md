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

## Update 01:24 Uruguay
Build 15 passed native takeoff at 04:06:40Z and full landing/stop at 04:07:54Z,
all 37 parts at 100 percent. Later throttle-release rolling check ran beyond the
runway and impacted terrain at 04:09:19Z; this is a separate test-fixture failure.
Build 17 repeated takeoff at 04:21:58Z. Internal gun fired 77 rounds (1500 to1423)
at 04:23:06-09Z after gear retraction, all airframe parts remained 100 percent.
Gear-down native weapon safety correctly prevented firing before retraction.
Airbrake remained open despite engine test inputs. FilterInputs postfix alone
did not fix input ordering. Test-only Airbrake.Update prefix now synchronizes
the same commanded inputs immediately before native airbrake evaluation.
Build 18 removes the internal gun pod donor capsule; native nose hull remains.
Bundle SHA256 F31319256A8D94D939A7D883A25EA46EC1653F954F3131EC11290AE77BCBC80D.
Final performance and external weapon tests remain pending. No playable release
or NOMM submission yet. Start Mirage 2000 only after A37 acceptance is complete.
