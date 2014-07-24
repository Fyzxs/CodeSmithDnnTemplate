using System;
using System.ComponentModel;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;

public class Utils
{
    public static string SplitCamelCase(string CamelCaseString)
    {
        return System.Text.RegularExpressions.Regex.Replace(CamelCaseString,
            "([A-Z])",
            "$1",
            System.Text.RegularExpressions.RegexOptions.Compiled
            ).Trim();
    }
}

public partial class MasterCodeBehind
{
    public static string ViewControl = "ViewControl";
    public static string DefaultPermissions = ";View;Edit;";
    public static string ExtendedPermissions = ";Content;Delete;Export;Import;Manage;";

    public static class Quick
    {
        public static string CallForXML(string type, string typeVer, string netVer, params object[] args)
        {
            return Call(type, typeVer, netVer, "<!--", "-->", args);
        }
        public static string CallForCS(string type, string typeVer, string netVer, params object[] args)
        {
            return Call(type, typeVer, netVer, "/*", "*/", args);
        }
        private static string Call(string type, string typeVer, string netVer, string nullPre, string nullPost, params object[] args)
        {
            string functionName = new StackTrace().GetFrame(2).GetMethod().Name;
            string targetFunctionName = functionName + "_" + type + "_" + typeVer + "_" + netVer;
            
            return DoInvoke(targetFunctionName, nullPre, nullPost, args);
        }
        
        public static string CallForCS(string targetFunctionName, params object[] args)
        {
            return Call(targetFunctionName, "/*", "*/", args);
        }
        
        public static string CallForXML(string targetFunctionName, params object[] args)
        {
            return Call(targetFunctionName, "<!--", "-->", args);
        }
        
        private static string Call(string targetFunctionName, string nullPre, string nullPost, params object[] args)
        {
            return DoInvoke(targetFunctionName, nullPre, nullPost, args);
        }
        private static string DoInvoke(string targetFunctionName, string nullPre, string nullPost, params object[] args)
        {
            Type type = new System.Diagnostics.StackTrace().GetFrame(3).GetMethod().ReflectedType;
            string className = type.FullName;
            MethodInfo mi = type.GetMethod(targetFunctionName, BindingFlags.Static | BindingFlags.NonPublic);
            return (mi == null) ? nullPre + " " + targetFunctionName + " is not set up. " + nullPost : mi.Invoke(null, args).ToString();
        }
    }

    #region Data Access Layer
    public static class DataAccessLayer
    {
        public enum Types
        {
            None,
            netTiers,
            nHibernate
        }
        
        public static string Usings(Types type, string location)
        {
            switch(type)
            {
                case Types.None: return String.Empty;
                case Types.netTiers:
                    return Usings_netTiers(location);
                default:
                    return "//Usings for " + type + " is not set up";
            }
        }
        private static string Usings_netTiers(string location)
        {
            string prjName = new System.IO.DirectoryInfo(location).Name;
            
            return @"using " + prjName + @".Services;
using " + prjName + @".Entities;
using " + prjName + @".Entities.Validation;
using " + prjName + @".Data;
using " + prjName + @".Data.Bases;
using " + prjName + @".Data.SqlClient;";
        }
        
        public static string ProjectReference(Types type, dotNet.Version netVersion, string location)
        {
            switch(type)
            {
                case Types.None: return String.Empty;
                case Types.netTiers:
                    return ProjectReference_netTiers(netVersion, location);
                default:
                    return "<!-- Project Reference for " + type + " is not set up -->";
            }
        }
        private static string ProjectReference_netTiers(dotNet.Version netVersion, string location)
        {   
            string proj = new System.IO.DirectoryInfo(location).Name;
            return @"<Reference Include=""" + proj + @".Entities"">
      <HintPath>./References/" + proj + @".Entities.dll</HintPath>
    </Reference>
    <Reference Include=""" + proj + @".Data"">
      <HintPath>./References/" + proj + @".Data.dll</HintPath>
    </Reference>
    <Reference Include=""" + proj + @".Data.SqlClient"">
      <HintPath>./References/" + proj + @".Data.SqlClient.dll</HintPath>
    </Reference>
    <Reference Include=""" + proj + @".Services"">
      <HintPath>./References/" + proj + @".Services.dll</HintPath>
    </Reference>";
        }
        
        
        public static void Copy(string srcDir, string dirRef, Types type, dotNet.Version netVersion)
        {
            switch(type)
            {
                case Types.netTiers:
                    Copy_netTiers(srcDir, dirRef, netVersion);
                    break;
            }
        }
        private static void Copy_netTiers(string srcDir, string dirRef, dotNet.Version netVersion)
        {
            string proj = new System.IO.DirectoryInfo(srcDir).Name;

            System.IO.File.Copy(srcDir + @"\" + proj + @".Entities\bin\Release\" + proj + ".Entities.dll", dirRef + @"\" + proj + ".Entities.dll", true);
            System.IO.File.Copy(srcDir + @"\" + proj + @".Entities\bin\Release\" + proj + ".Entities.xml", dirRef + @"\" + proj + ".Entities.xml", true);
            
            System.IO.File.Copy(srcDir + @"\" + proj + @".Data.SqlClient\bin\Release\" + proj + ".Data.SqlClient.dll", dirRef + @"\" + proj + ".Data.SqlClient.dll", true);
            System.IO.File.Copy(srcDir + @"\" + proj + @".Data.SqlClient\bin\Release\" + proj + ".Data.SqlClient.xml", dirRef + @"\" + proj + ".Data.SqlClient.xml", true);
            
            System.IO.File.Copy(srcDir + @"\" + proj + @".Data\bin\Release\" + proj + ".Data.dll", dirRef + @"\" + proj + ".Data.dll", true);
            System.IO.File.Copy(srcDir + @"\" + proj + @".Data\bin\Release\" + proj + ".Data.xml", dirRef + @"\" + proj + ".Data.xml", true);
            
            System.IO.File.Copy(srcDir + @"\" + proj + @".Services\bin\Release\" + proj + ".Services.dll", dirRef + @"\" + proj + ".Services.dll", true);
            System.IO.File.Copy(srcDir + @"\" + proj + @".Services\bin\Release\" + proj + ".Services.xml", dirRef + @"\" + proj + ".Services.xml", true);
            
        }
    }
    #endregion
    
    #region Source Control
    public static class SourceControl
    {
        public enum Types
        {
            None,
            Hg,
            SVN            
        }
        
        public enum HgPluginTypes
        {
            None,
            HgSccPackage
        }
    
        public static string BuildFile_Include(Types type)
        {
            switch(type)
            {
                case Types.None:return String.Empty;
                case Types.Hg:
                    return BuildFile_Include_Hg();
                default:
                    return "<!-- Build File Include for " + type + " is not set up -->";
            }
        }
        private static string BuildFile_Include_Hg()
        {
            return @"	<!-- Include MSBuild Mercurial Tasks.-->
	<Choose>
		<When Condition=""Exists('$(MSBuildExtensionsPath)\MSBuild.Mercurial\MSBuild.Mercurial.Tasks')"">
			<PropertyGroup>
				<MSBuildMercurialTasksAvailable>true</MSBuildMercurialTasksAvailable>
			</PropertyGroup>
		</When>
		<Otherwise>
			<PropertyGroup>
				<MSBuildMercurialTasksAvailable>false</MSBuildMercurialTasksAvailable>
			</PropertyGroup>
		</Otherwise>
	</Choose>

	<Import Project=""$(MSBuildExtensionsPath)\MSBuild.Mercurial\MSBuild.Mercurial.Tasks""
			Condition="" '$(MSBuildMercurialTasksAvailable)' == 'true' ""/>";
        }
        
        public static string BuildFile_CallTarget(Types type)
        {
            switch(type)
            {
                case Types.None:return String.Empty;
                case Types.Hg:
                    return BuildFile_CallTarget_Hg();
                default:
                    return "<!-- Build File CallTarget for " + type + " is not set up -->";
            }
        }
        private static string BuildFile_CallTarget_Hg()
        {
            return @"<CallTarget Targets=""Hg""></CallTarget>";
        }
        
        public static string BuildFile_Targets(Types type)
        {
            switch(type)
            {
                case Types.None:return String.Empty;
                case Types.Hg:
                    return BuildFile_Targets_Hg();
                default:
                    return "<!-- Build File Targets for " + type + " is not set up -->";
            }            
        }
        private static string BuildFile_Targets_Hg()
        {
            return @"   <!--Start Mecurial-->
	<Target Name=""Hg"" Condition=""'$(MSBuildMercurialTasksAvailable)' == 'true'"">
		<CallTarget Targets=""HgCommit""></CallTarget>
	</Target>
	
	<!-- Commit the Changes to our Local Repository -->
	<Target Name=""HgCommit"">
		<CreateProperty Value=""$(ConfigurationName) Compile Commit $(Major).$(Minor).$(Build).$(Revision) by $(username) on $(computername)"">
			<Output TaskParameter=""Value"" PropertyName=""CommitMessage"" />
		</CreateProperty>
		<HgCommit AddRemove=""true"" LocalPath=""$(MSBuildProjectDirectory)"" Message=""$(CommitMessage)"" User=""$(UserName)"" />
	</Target>";
        }
        
        public static string ProjectFile_Plugin(Types type, HgPluginTypes hgPluginType/*, SvnPluginTypes svnTypes*/)
        {
            switch(type)
            {
                case Types.None:return String.Empty;
                case Types.Hg:
                    return ProjectFile_Plugin_Hg(hgPluginType);
                default:
                    return "<!-- Project File Plugin for " + type + " is not set up -->";
            }             
        }
        private static string ProjectFile_Plugin_Hg(HgPluginTypes hgPluginType)
        {
            switch(hgPluginType)
            {
                case HgPluginTypes.None:return String.Empty;
                case HgPluginTypes.HgSccPackage:
                    return ProjectFile_Plugin_Hg_HgSccPackage();
                default:
                    return "<!-- Project File Plugin Hg for " + hgPluginType + " is not set up -->";
            } 
        }
        private static string ProjectFile_Plugin_Hg_HgSccPackage()
        {
            return @"    <SccProjectName>&lt;Project Location In Database&gt;</SccProjectName>
    <SccLocalPath>&lt;Local Binding Root of Project&gt;</SccLocalPath>
    <SccAuxPath>&lt;Source Control Database&gt;</SccAuxPath>
    <SccProvider>Mercurial Source Control Package</SccProvider>
    <TargetFrameworkProfile />";
        }
        
        public static void Copy(string srcDir, string dirRoot, Types type, dotNet.Version netVersion, HgPluginTypes hgPluginType/*, Other types*/)
        {
            switch(type)
            {
                case Types.Hg:
                    Copy_Hg(srcDir, dirRoot, netVersion, hgPluginType);
                    break;
            }
        }
        public static void Copy_Hg(string srcDir, string dirRoot, dotNet.Version netVersion, HgPluginTypes hgPluginType)
        {
            switch(hgPluginType)
            {
                case HgPluginTypes.HgSccPackage:
                    System.IO.File.Copy(srcDir + @"\References\SourceControl\Hg\.hgignore", dirRoot + @"\.hgignore", true);
                    Copy_Hg_HgSccPackage(srcDir, dirRoot, netVersion);
                    break;
            }
        }
        
        public static void Copy_Hg_HgSccPackage(string srcDir, string dirRef, dotNet.Version netVersion)
        {
            //This is chilling as an example.
        }
    }
    
    
    #endregion
    
    #region .Net Version
    
    public static class dotNet
    {
        public enum Version
        {
            v3_5,
            v4_0
        }
        
        public static string ToString(Version version)
        {
            switch(version)
            {
                case Version.v3_5:
                    return "v3.5";
                case Version.v4_0:
                default:
                    return "v4.0";
            }
        }
    }
    #endregion
    
    #region Remote Bug Submission
    public static class RemoteBugSub
    {
        public enum Types
        {
            None,
            FogBugz,
            Insight
        }
        
        //Technically BugzScount
        public enum FogBugzVersion
        {
            v1_0_2423_17548,
            v1_0_3481_18140
        }        
        public static string SubmissionDeclaration(Types bugSubType, dotNet.Version netVersion)
        {
            switch(bugSubType)
            {
                case Types.None:return String.Empty;
                case Types.FogBugz:
                    return SubmissionDeclaration_FogBugz(netVersion);
                default:
                    return "//Remote Bug Submission for " + bugSubType + " is not set up";
            }
        }        
        private static string SubmissionDeclaration_FogBugz(dotNet.Version netVersion)
        {
            switch(netVersion)
            {
                case dotNet.Version.v4_0:
                    return SubmissionDeclaration_FogBugz_v4();
                default:
                    return "//Fog Bugz Remote Submission for .Net " + netVersion + " is not set up";
            }
            
        }
        private static string SubmissionDeclaration_FogBugz_v4()
        {
            return @"#region Remote Bug Submission
    internal static class BugzScout
    {
        public static void Submit(string message,
                                  string url = ""https:\\CompanyName.fogbugz.com/ScoutSubmit.asp"", 
                                  string username = ""BugSubmitter"",
                                  string project =  ""ModuleTemplate"", 
                                  string area = ""Misc"", 
                                  string email = ""noEmail@127.0.0.1"",
                                  string versionFormat = ""{0}.{1}.{2}.{3}"",
                                  bool forceNewBug = false, Exception exception = null, 
                                  bool addUserAndMachineInfo = false, bool appendAssemblyList = false)
        {
            FogBugz.BugReport.Submit(url, username, project, area, email, forceNewBug, message, exception,
                                     addUserAndMachineInfo, versionFormat, appendAssemblyList);
        }
    }
    #endregion";
        }
        
        public static string DefaultWithException(Types bugSubType, dotNet.Version netVersion)
        {
            switch(bugSubType)
            {
                case Types.None:return String.Empty;
                case Types.FogBugz:
                    return DefaultWithException_FogBugz(netVersion);
                default:
                    return "//Remote Bug Submission for " + bugSubType + " is not set up";
            }
        }

        private static string DefaultWithException_FogBugz(dotNet.Version netVersion)
        {
            switch(netVersion)
            {
                case dotNet.Version.v4_0:
                    return DefaultWithException_FogBugz_v4();
                default:
                    return "//Fog Bugz Remote Submission for .Net " + netVersion + " is not set up";
            }
        }
        private static string DefaultWithException_FogBugz_v4()
        {
            return @"#if !DEBUG
                BugzScout.Submit(""DnnUserId ["" + Module.UserId + ""] encounted the following Exception..."", exception: ex);
#endif";
        }
        
        
        public static string ProjectReference(Types bugSubType, dotNet.Version netVersion)
        {
            switch(bugSubType)
            {
                case Types.None:return String.Empty;
                case Types.FogBugz:
                    return ProjectReference_FogBugz(netVersion);
                default:
                    return "//Remote Bug Submission for " + bugSubType + " is not set up";
            }
        }
        private static string ProjectReference_FogBugz(dotNet.Version netVersion)
        {
            return @"<Reference Include=""BugzScoutDotNet"">
      <HintPath>./References/BugzScoutDotNet.dll</HintPath>
    </Reference>";
            
        }
        
        
        public static void Copy(string srcDir, string dirRef, Types bugSubType, dotNet.Version netVersion, FogBugzVersion fogBugzVer)
        {
            switch(bugSubType)
            {
                case Types.FogBugz:
                    Copy_FogBugz(srcDir, dirRef, netVersion, fogBugzVer);
                    break;
            }
        }
        private static void Copy_FogBugz(string srcDir, string dirRef, dotNet.Version netVersion, FogBugzVersion fogBugzVer)
        {
            switch(fogBugzVer)
            {
                case FogBugzVersion.v1_0_2423_17548: 
                    System.IO.File.Copy(srcDir + @"\References\RemoteBugSubmission\FogBugz\1.0.2423.17548\BugzScoutDotNet.dll", dirRef + @"\BugzScoutDotNet.dll", true);
                    break;
                case FogBugzVersion.v1_0_3481_18140: 
                    System.IO.File.Copy(srcDir + @"\References\RemoteBugSubmission\FogBugz\1.0.3481.18140\BugzScoutDotNet.dll", dirRef + @"\BugzScoutDotNet.dll", true);
                    break;
            }           
        }
        
        
    }
    #endregion
    
    #region Logging
    public static class Logging
    {
        public enum Types
        {
            None,
            log4net,
            EnterpriseLibrary
        }
        public enum log4netVersion
        {
            v1_2_10
        }
        
        public static string LoggingDeclaration(Types logType, dotNet.Version netVersion)
        {
            switch(logType)
            {
                case Types.None: return String.Empty;
                case Types.log4net:
                    return LoggingDeclaration_log4net(netVersion);
                default:
                    return "//Logging for " + logType + " is not set up";
            }
        }
        private static string LoggingDeclaration_log4net(dotNet.Version netVersion)
        {
          return @"#region Log
    internal static class LogWrap
    {
        public static bool IsDebugEnabled { get { return Log.IsDebugEnabled; } }
        public static bool IsErrorEnabled { get { return Log.IsErrorEnabled; } }
        public static bool IsFatalEnabled { get { return Log.IsFatalEnabled; } }
        public static bool IsInfoEnabled { get { return Log.IsInfoEnabled; } }
        public static bool IsWarnEnabled { get { return Log.IsWarnEnabled; } }

        public static log4net.ILog Log;

        public static void InitLogging(int dnnUserId, Type type)
        {
            //http://stackoverflow.com/questions/571876/best-way-to-dynamically-set-an-appender-file-path
            //<file type=""log4net.Util.PatternString"" value=""%property{LogPath}%property{LogName}"" /> - The type is important
            //conversionPattern -- %date [%thread] %-5level %c{2} DnnUserId:%property{DnnUserId} %message%newline%exception
            log4net.GlobalContext.Properties[""LogName""] = Definition.ModuleName;
            //ApplicationMapPath does not end with /
            log4net.GlobalContext.Properties[""LogPath""] = DotNetNuke.Common.Globals.ApplicationMapPath + @""\"" + @""DesktopModules"" + @""\"" + @""Pwner"" + @""\_Logging"";
            log4net.ThreadContext.Properties[""DnnUserId""] = dnnUserId;

            Log = log4net.LogManager.GetLogger(type);

            log4net.Config.XmlConfigurator.Configure();
        }
        public static void DebugEnter()
        {
            if (Log.IsDebugEnabled) LogWrap.Log.Debug(LogWrap.Function(""Enter""));
        }
        public static void DebugExit()
        {
            if (Log.IsDebugEnabled) LogWrap.Log.Debug(LogWrap.Function(""Exit""));
        }
        private static string Function(string status)
        {
            return new System.Diagnostics.StackTrace().GetFrame(2).GetMethod().Name + status;
        }
    }
    #endregion";
        }
        
        public static string InitializeLoggingCall(Types logType, dotNet.Version netVersion)
        {
            string retVal = "";
            switch(logType)
            {
                case Types.None:break;
                case Types.log4net:
                    retVal = "LogWrap.InitLogging(Module.UserId, System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);";
                break;
                default:
                    retVal = "//Logging for " + logType + " is not set up";
                break;
            }
                
            return retVal;
        }
        
        public static string DebugEnter(Types logType, dotNet.Version netVersion)
        {
            string retVal = "";
            switch(logType)
            {
                case Types.None:break;
                case Types.log4net:
                    retVal = "LogWrap.DebugEnter();";
                break;
                default:
                    retVal = "//Logging for " + logType + " is not set up";
                break;
            }
                
            return retVal;
        }
        public static string DebugExit(Types logType, dotNet.Version netVersion)
        {
            string retVal = "";
            switch(logType)
            {
                case Types.None:break;
                case Types.log4net:
                    retVal = "LogWrap.DebugExit();";
                break;
                default:
                    retVal = "//Logging for " + logType + " is not set up";
                break;
            }
                
            return retVal;
        } 
        public static string LogDebug(Types logType, dotNet.Version netVersion, string msg)
        {
            return Log("Debug", logType, netVersion, msg);
        }
        public static string LogInfo(Types logType, dotNet.Version netVersion, string msg)
        {
            return Log("Info", logType, netVersion, msg);
        }
        public static string LogFatal(Types logType, dotNet.Version netVersion, string msg)
        {
            return Log("Fatal", logType, netVersion, msg);
        }
        private static string Log(string type, Types logType, dotNet.Version netVersion, string msg)
        {
            switch(logType)
            {
                case Types.None:return String.Empty;
                case Types.log4net:
                    return Log_log4net(type, netVersion, msg);
                default:
                    return "//Logging for " + logType + " is not set up";
            }
        }
        private static string Log_log4net(string type, dotNet.Version netVersion, string msg)
        {
            switch(netVersion)
            {
                case dotNet.Version.v4_0:
                    return Log_log4net_v4(type, msg);
                default:
                    return "//Log log4net for .Net " + netVersion + " is not set up";
            }
        }
        private static string Log_log4net_v4(string type, string msg)
        {
            return "if (LogWrap.Is" + type + "Enabled) LogWrap.Log." + type + "(@" + msg + ");";
        }
        
        public static string ProjectReference(Types logType, dotNet.Version netVersion)
        {
            switch(logType)
            {
                case Types.None:return String.Empty;
                case Types.log4net:
                    return ProjectReference_log4net(netVersion);
                default:
                    return "<!-- Logging for " + logType + " is not set up -->";
            }
        }
        private static string ProjectReference_log4net(dotNet.Version netVersion)
        {
            return @"<Reference Include=""log4net"">
      <HintPath>./References/log4net.dll</HintPath>
    </Reference>";
            
        }
    
        public static void Copy(string srcDir, string dirRef, Types logType, dotNet.Version netVersion, log4netVersion log4netVer/*, Other log types*/)
        {
            switch(logType)
            {
                case Types.log4net:
                    Copy_log4net(srcDir, dirRef, netVersion, log4netVer);
                    break;
            }
        }
        private static void Copy_log4net(string srcDir, string dirRef, dotNet.Version netVersion, log4netVersion log4netVer)
        {
            switch(log4netVer)
            {
                case log4netVersion.v1_2_10: 
                    System.IO.File.Copy(srcDir + @"\References\Logging\log4net\1.2.10\log4net.dll", dirRef + @"\log4net.dll", true);
                    System.IO.File.Copy(srcDir + @"\References\Logging\log4net\1.2.10\log4net.xml", dirRef + @"\log4net.xml", true);
                    break;
            }           
        }
        
        public static string ManifestEntry(string owner, Types logType)
        {
            return Quick.CallForXML("ManifestEntry_" + logType, owner);           
        }
        private static string ManifestEntry_log4net(string owner)
        {
            return @"<component type=""Config"">
          <config>
            <configFile>web.config</configFile>
            <install>
              <configuration>
                <nodes>
                  <node path=""/configuration/configSections"" action=""update"" key=""name"" collision=""overwrite"">
                    <section name=""log4net"" type=""log4net.Config.Log4NetConfigurationSectionHandler, log4net"" />
                  </node>
                  <node path=""/configuration"" action=""update"" key=""key"" collision=""overwrite"">
                    <log4net key=""log4net"">
                    </log4net>
                  </node>
                  <node path=""/configuration/log4net"" action=""update"" key=""key"" collision=""overwrite"">
                    <root key=""root"">
                    </root>
                  </node>
                  <node path=""/configuration/log4net/root"" key=""ref"" action=""update"" collision=""overwrite"">
                    <appender-ref ref=""" + owner + @"DefaultFileAppender"" />
                  </node>
                  <node path=""/configuration/log4net"" key=""name"" action=""update"" collision=""overwrite"">
                    <appender name=""" + owner + @"DefaultFileAppender"" type=""log4net.Appender.FileAppender"">
                      <file type=""log4net.Util.PatternString"" value=""~/%property{LogPath}/%property{LogName}"" />
                      <appendToFile value=""true"" />
                      <lockingModel type=""log4net.Appender.FileAppender+MinimalLock"" />
                      <layout type=""log4net.Layout.PatternLayout"">
                        <conversionPattern value=""%date [%thread] %-5level %c{2} DnnUserId:%property{DnnUserId} %message%newline%exception"" />
                      </layout>
                    </appender>
                  </node>
                </nodes>
              </configuration>
            </install>
            <uninstall>
              <configuration>
                <nodes>
                  <node path=""/configuration/log4net/appender[@name='" + owner + @"DefaultFileAppender']"" action=""remove"" />
                  <node path=""/configuration/log4net/root/appender-ref[@ref='" + owner + @"DefaultFileAppender']"" action=""remove"" />
                </nodes>
              </configuration>
            </uninstall>
          </config>
        </component>";
        }
        
    }
    #endregion
    
    #region Unit Testing
        /*
            I've opted not to give options ATM for unit testing.
            DotNetNuke Corp. has gone with mbUnit and Waitin as their testing choices, I'm only coding up those templates.
            I've tried to give the framework to have options for the rest.
            Each option will, essentially, require it's own set of files. Since it's such a massive change, and not
            just a small chunk in a file - It's not 'swappable'.
            If something is later developed to make it easy, sure, it can be added. For now - Not doing it.
            
            Also - I'm not happy with the depth of functions that setting up the options force.
            I understand it's the cost of flexibility, but it annoys me. Feels like code duplication.
            
            I've fixed the depth issue with Quick.Call.
        */
        /*#if !QUNIT
            if (LogWrap.IsDebugEnabled) LogWrap.Log.Debug(@"Registering the javascript file QUnit)");
            Generic.RegisterJavascript(Page, "QUnit", "http://github.com/jquery/qunit/raw/master/qunit/qunit.js");
                
            if (LogWrap.IsDebugEnabled) LogWrap.Log.Debug(@"Registering the javascript file QUnit)");
            Generic.RegisterJavascript(Page, "ViewControlQUnitTest", ResolveUrl("js/ViewControlQUnitTest.js"));

            ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), "runViewControlQUnitTest",
                                                    " $(document).ready(function(){Pwner.TestProj.ViewControl.UnitTests.runTests();});",
                                                    true);

            var link = new HtmlGenericControl("LINK");
            link.Attributes.Add("rel", "stylesheet");
            link.Attributes.Add("type", "text/css");
            link.Attributes.Add("href", "http://github.com/jquery/qunit/raw/master/qunit/qunit.css");
            Controls.Add(link); ;

            divValidation.Parent.Controls.AddAt(0, new System.Web.UI.LiteralControl(@"<h1 id=""qunit-header"">QUnit example</h1>"
                                                                           + @"<h2 id=""qunit-banner""></h2>"
                                                                           + @"<h2 id=""qunit-userAgent""></h2>"
                                                                           + @"<ol id=""qunit-tests""></ol>"));

#endif
*/
    #endregion
}