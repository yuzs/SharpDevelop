// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Daniel Grunwald" email="daniel@danielgrunwald.de"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.IO;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Project;
using Microsoft.Build.Framework;

namespace ICSharpCode.CodeAnalysis
{
	/// <summary>
	/// Not all FxCop messages come with correct line number information,
	/// so this logger fixes the position.
	/// Additionally, it registers the context menu containing the 'suppress message' command.
	/// </summary>
	public class FxCopLogger : IMSBuildLoggerFilter
	{
		public IMSBuildChainedLoggerFilter CreateFilter(MSBuildEngine engine, IMSBuildChainedLoggerFilter nextFilter)
		{
			engine.OutputTextLine(StringParser.Parse("${res:ICSharpCode.CodeAnalysis.RunningFxCopOn} " + Path.GetFileNameWithoutExtension(engine.ProjectFileName)));
			return new FxCopLoggerImpl(engine, nextFilter);
		}
		
		sealed class FxCopLoggerImpl : IMSBuildChainedLoggerFilter
		{
			readonly MSBuildEngine engineWorker;
			readonly IMSBuildChainedLoggerFilter nextChainElement;
			
			public FxCopLoggerImpl(MSBuildEngine engineWorker, IMSBuildChainedLoggerFilter nextChainElement)
			{
				this.engineWorker = engineWorker;
				this.nextChainElement = nextChainElement;
			}
			
			public void HandleError(BuildError error)
			{
				LoggingService.Debug("FxCopLogger got " + error.ToString());
				
				string[] moreData = (error.Subcategory ?? "").Split('|');
				string checkId = error.ErrorCode;
				error.ErrorCode = (error.ErrorCode != null) ? error.ErrorCode.Split(':')[0] : null;
				if (FileUtility.IsValidPath(error.FileName) &&
				    Path.GetFileName(error.FileName) == "SharpDevelop.CodeAnalysis.targets")
				{
					error.FileName = null;
				}
				IProject project = ProjectService.GetProject(engineWorker.ProjectFileName);
				if (project != null) {
					IProjectContent pc = ParserService.GetProjectContent(project);
					if (pc != null) {
						if (error.FileName != null && error.FileName.StartsWith("positionof#")) {
							string memberName = error.FileName.Substring(11);
							FilePosition pos = GetPosition(pc, memberName);
							if (pos.IsEmpty == false && pos.CompilationUnit != null) {
								error.FileName = pos.FileName ?? "";
								error.Line = pos.Line;
								error.Column = pos.Column;
							} else {
								error.FileName = null;
							}
						}
						
						if (moreData.Length > 1 && !string.IsNullOrEmpty(moreData[0])) {
							error.Tag = new FxCopTaskTag {
								ProjectContent = pc,
								TypeName = moreData[0],
								MemberName = moreData[1],
								Category = error.HelpKeyword,
								CheckID = checkId
							};
						} else {
							error.Tag = new FxCopTaskTag {
								ProjectContent = pc,
								Category = error.HelpKeyword,
								CheckID = checkId
							};
						}
						error.ContextMenuAddInTreeEntry = "/SharpDevelop/Pads/ErrorList/CodeAnalysisTaskContextMenu";
						if (moreData.Length > 2) {
							(error.Tag as FxCopTaskTag).MessageID = moreData[2];
						}
					}
				}
				nextChainElement.HandleError(error);
			}
			
			public void HandleBuildEvent(Microsoft.Build.Framework.BuildEventArgs e)
			{
				nextChainElement.HandleBuildEvent(e);
			}
			
			static FilePosition GetPosition(IProjectContent pc, string memberName)
			{
				// memberName is a special syntax used by our FxCop task:
				// className#memberName
				int pos = memberName.IndexOf('#');
				if (pos <= 0)
					return FilePosition.Empty;
				string className = memberName.Substring(0, pos);
				memberName = memberName.Substring(pos + 1);
				return SuppressMessageCommand.GetPosition(pc, className, memberName);
			}
		}
	}
	
	public class FxCopTaskTag
	{
		public IProjectContent ProjectContent { get; set; }
		public string TypeName { get; set; }
		public string MemberName { get; set; }
		public string Category { get; set; }
		public string CheckID { get; set; }
		public string MessageID { get; set; }
	}
}
