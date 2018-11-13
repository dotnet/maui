var arch = Is64Bit ? "x64" : "x86";

SetPowershellExecutionPolicy (PowershellExecutionPolicy.Bypass, PowershellScope.CurrentUser);

SetUACBehavior (UACBehavior.ElevateWithoutConsentOrCredentials);

InstallWindowsFeature ("NetFx3");

VisualStudio (VisualStudioChannel.Stable, VisualStudioTier.Enterprise, "15.8.9")
  .Workload (VisualStudioWorkload.ManagedDesktop)
  .Workload (VisualStudioWorkload.NetCrossPlat)
  .Workload (VisualStudioWorkload.NativeDesktop)
  .Workload (VisualStudioWorkload.Universal)
  .Component (VisualStudioComponent.Microsoft_VisualStudio_Component_Windows81SDK)
  .Component (VisualStudioComponent.Microsoft_VisualStudio_Component_Windows10SDK_16299_UWP)
  .Component (VisualStudioComponent.Microsoft_VisualStudio_Component_Windows10SDK_16299_UWP_Native)
  .Component (VisualStudioComponent.Component_Android_SDK23)
  .Component (VisualStudioComponent.Component_Android_SDK25)
  .Component (VisualStudioComponent.Component_JavaJDK)
  .Component (VisualStudioComponent.Microsoft_Net_Component_4_5_1_TargetingPack)
  .Component (VisualStudioComponent.Microsoft_Net_Component_4_5_2_TargetingPack)
  .Component (VisualStudioComponent.Microsoft_Net_Component_4_5_TargetingPack)
  .Component (VisualStudioComponent.Microsoft_Net_Component_4_6_2_SDK)
  .Component (VisualStudioComponent.Microsoft_Net_Component_4_6_2_TargetingPack)
  .Component (VisualStudioComponent.Microsoft_Net_Component_4_7_SDK)
  .Component (VisualStudioComponent.Microsoft_Net_Component_4_7_TargetingPack);