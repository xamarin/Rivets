using System.Reflection;
using System.Runtime.CompilerServices;

// Information about this assembly is defined by the following attributes.
// Change them to the values specific to your project.

[assembly: AssemblyTitle (AssemblyInfo.AssemblyTitle)]
[assembly: AssemblyDescription (AssemblyInfo.AssemblyDescription)]
[assembly: AssemblyConfiguration ("")]
[assembly: AssemblyCompany ("Xamarin")]
[assembly: AssemblyProduct ("")]
[assembly: AssemblyCopyright ("Xamarin")]
[assembly: AssemblyTrademark ("")]
[assembly: AssemblyCulture ("")]

// The assembly version has the format "{Major}.{Minor}.{Build}.{Revision}".
// The form "{Major}.{Minor}.*" will automatically update the build and revision,
// and "{Major}.{Minor}.{Build}.*" will update just the revision.

[assembly: AssemblyVersion (AssemblyInfo.AssemblyVersion)]

// The following attributes are used to specify the signing key for the assembly,
// if desired. See the Mono documentation for more information about signing.

//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile("")]

internal class AssemblyInfo
{
	public const string AssemblyVersion = "1.0.3";

	public const string AssemblyTitle = "Rivets";
	public const string AssemblyProduct = "Rivets";
	public const string AssemblyDescription = "A C# implementation of App Links, functionally, a port of Bolts";
}
