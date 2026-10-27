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

## Update 01:33 Uruguay
Build18: all three wheels contact after capsule removal; takeoff passed04:28:53Z.
Airbrake.Update test-input synchronization verified openAmount0 under power.
Gun fired78rounds (1500to1422), all37parts100. Three native camera states observed
with health unchanged. Clean speed exceeded850km/h at~1720m: too fast for target.
Repositioning from far away/high speed for landing broke joints immediately on
fixture relocation04:31:25Z; this approach does NOT pass. Repeat near base before
changing the aircraft. Earlier build15 landing pass remains separately recorded.
Build19 native engine thrust falloff begins205m/s (previous260), engineering fit,
not a hard speed clamp. Its performance still needs measurement.

## Build19 landing acceptance 01:39 Uruguay
New spawn: three native wheels contact, all37parts100. Takeoff/climb passed
04:38:25Z. Near-base approach fixture, gear left down, then native touchdown and
braking. Full stop04:39:26Z; all37parts100, no joint-failure trace.
Bundle SHA256 DF2AE1ABE258AB289DD00547069585CB7C025D1F433A82C50278FDBB8AA055DC.
Clean performance test now uses a separate new spawn; do not combine its fuel
and starting mass with the previous landing test.

Build19 clean speed settled221.00m/s=795.6km/h, altitude1690.8m, mass3418kg,
all37parts100, gearfold~1, airbrake0, no AB. Gun1500to1422rounds. Logged more
than40seconds near same speed/altitude. External weapon acceptance still pending.

## 01:55 final beta acceptance
Mixed8station spawn4796kg: takeoff passed04:53:03Z, PAB2502to0,
AGR18 14to11, PAB1254to2to0. All37airframeparts100 at04:55:20Z,
no failure trace. Local probe removed before normal play/distribution.
Known cosmetic/system limits remain documented in README. Build19 is a playable
beta, not a fully certified replica. Publication packaging follows this checkpoint.

## Publication 01:59 Uruguay
GitHub prerelease v0.1.0 uploaded and downloaded again; ZIP SHA256
67b0e3cd077fad99be199cd9bb69c6d646c252822beef400fd449f33f61e1b56 matches.
NOMM manifest schema valid, PR https://github.com/KopterBuzz/NOMNOM/pull/465
submitted open; registry acceptance is up to its maintainers. Test DLL removed.
User requested Mirage2000 next; only source/license research begins now.
