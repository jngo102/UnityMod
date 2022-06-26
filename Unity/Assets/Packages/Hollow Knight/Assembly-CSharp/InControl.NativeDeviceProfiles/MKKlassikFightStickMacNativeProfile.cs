namespace InControl.NativeDeviceProfiles
{
	
	[Preserve]
	[NativeInputDeviceProfile]
	public class MKKlassikFightStickMacNativeProfile : Xbox360DriverMacNativeProfile
	{
		public override void Define()
		{
			base.Define();
			base.DeviceName = "MK Klassik Fight Stick";
			base.DeviceNotes = "MK Klassik Fight Stick on Mac";
			base.Matchers = new InputDeviceMatcher[1]
			{
				new InputDeviceMatcher
				{
					VendorID = (ushort)4779,
					ProductID = (ushort)771
				}
			};
		}
	}
}