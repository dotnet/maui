using System;
using System.IO;
using System.Linq;
using Serilog;

if (!IsMac)
  return;

Log.Information ("Identity : " + Env(""APPLECODESIGNIDENTITYURL""));
Log.Information ("Profile : " + Env(""APPLECODESIGNPROFILEURL""));
AppleCodesignIdentity("iPhone Developer: Xamarin QA (JP4JS5NR3R)",Env("APPLECODESIGNIDENTITYURL"));
AppleCodesignProfile(Env("APPLECODESIGNPROFILEURL"));
