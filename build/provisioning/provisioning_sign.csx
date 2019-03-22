using System;
using System.IO;
using System.Linq;
using Serilog;

if (!IsMac)
  return;

AppleCodesignIdentity(Env("APPLECODESIGNIDENTITY"),Env("APPLECODESIGNIDENTITYURL"));
AppleCodesignProfile(Env("APPLECODESIGNPROFILEURL"));
