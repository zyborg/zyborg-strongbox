using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Zyborg.Vault.Version
{
	public static partial class Globals
	{
		//~ var (
		//~ 	// The git commit that was compiled. This will be filled in by the compiler.
		//~ 	GitCommit   string
		//~ 	GitDescribe string
		//~ 
		//~ 	Version           = "unknown"
		//~ 	VersionPrerelease = "unknown"
		//~ )
		// TODO: we need to come up with an alternative
		//       approach such as build-time "code-gen"

#if (!VERS_CODE_GEN)
		public static readonly string GitCommit;
		public static readonly string GitDescribe;
		public static readonly string Version = "unknown";
		public static readonly string VersionPrerelease = "unknown";

		// TODO: do we need this???
		//~ func init() {
		//~ 	// The main version number that is being run at the moment.
		//~ 	Version = "0.6.2"
		//~ 
		//~ 	// A pre-release marker for the version. If this is "" (empty string)
		//~ 	// then it means that it is a final release. Otherwise, this is a pre-release
		//~ 	// such as "dev" (in development), "beta", "rc1", etc.
		//~ 	VersionPrerelease = "dev"
		//~ }
		static Globals()
		{
			Version = "0.0.0";
			VersionPrerelease = "dev";
		}
#endif
	}

	// VersionInfo
	//~ type VersionInfo struct {
	public class VersionInfo
	{
		//~ Revision          string
		//~ Version           string
		//~ VersionPrerelease string
		public string Revision
		{ get; private set; }
		public string Version
		{ get; private set; }
		public string VersionPrerelease
		{ get; private set; }


		//~ func GetVersion() *VersionInfo {
		public static VersionInfo GetVersion()
		{
			//~ ver := Version
			//~ rel := VersionPrerelease
			//~ if GitDescribe != "" {
			//~ 	ver = GitDescribe
			//~ }
			//~ if GitDescribe == "" && rel == "" && VersionPrerelease != "" {
			//~ 	rel = "dev"
			//~ }
			var ver = Globals.Version;
			var rel = Globals.VersionPrerelease;
			if (!string.IsNullOrEmpty(Globals.GitDescribe))
				ver = Globals.GitDescribe;
			if (string.IsNullOrEmpty(Globals.GitDescribe) && string.IsNullOrEmpty(rel)
					&& !string.IsNullOrEmpty(Globals.VersionPrerelease))
				rel = "dev";

			//~ return &VersionInfo{
			//~ 	Revision:          GitCommit,
			//~ 	Version:           ver,
			//~ 	VersionPrerelease: rel,
			//~ }
			return new VersionInfo
			{
				Revision = Globals.GitCommit,
				Version = ver,
				VersionPrerelease = rel,
			};
		}

		//~ func (c *VersionInfo) VersionNumber() string {
		public string VersionNumber()
		{
			//~ if Version == "unknown" && VersionPrerelease == "unknown" {
			//~ 	return "(version unknown)"
			//~ }
			if (Globals.Version == "unknown" && Globals.VersionPrerelease == "unknown")
			{
				return "(version unknown)";
			}

			//~ version := fmt.Sprintf("%s", c.Version)
			var version = Version;

			//~ if c.VersionPrerelease != "" {
			//~ 	version = fmt.Sprintf("%s-%s", version, c.VersionPrerelease)
			//~ }
			if (!string.IsNullOrEmpty(VersionPrerelease))
				version = $"{version}-{VersionPrerelease}";

			//~ return version
			return version;
		}

		//~ func (c *VersionInfo) FullVersionNumber() string {
		public string FullVersionNumber()
		{
			//~var versionString bytes.Buffer

			//~ if Version == "unknown" && VersionPrerelease == "unknown" {
			//~ 	return "Vault (version unknown)"
			//~ }
			if (Globals.Version == "unknown" && Globals.VersionPrerelease == "unkonwn")
				return "Vault (version unknown)";

			//~ fmt.Fprintf(&versionString, "Vault v%s", c.Version)
			//~ if c.VersionPrerelease != "" {
			//~ 	fmt.Fprintf(&versionString, "-%s", c.VersionPrerelease)
			//~ 
			//~ 	if c.Revision != "" {
			//~ 		fmt.Fprintf(&versionString, " (%s)", c.Revision)
			//~ 	}
			//~ }
			var versionString = $"Vault v{Version}";
			if (!string.IsNullOrEmpty(VersionPrerelease))
			{
				versionString += $"-{VersionPrerelease}";
				if (!string.IsNullOrEmpty(Revision))
					versionString += $" ({Revision})";
			}

			//~ return versionString.String()
			return versionString;
		}
	}
}
