﻿// <file>
//     <copyright see="prj:///doc/copyright.txt">2002-2005 AlphaSierraPapa</copyright>
//     <license see="prj:///doc/license.txt">GNU General Public License</license>
//     <owner name="David Srbecký" email="dsrbecky@gmail.com"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Collections.Generic;

using Debugger.Interop.CorDebug;

namespace Debugger
{
	public partial class NDebugger
	{
		int lastAssignedModuleOrderOfLoading= 0;

		List<Module> moduleCollection = new List<Module>();

		public event EventHandler<ModuleEventArgs> ModuleLoaded;
		public event EventHandler<ModuleEventArgs> ModuleUnloaded;

		protected void OnModuleLoaded(Module module)
		{
			if (ModuleLoaded != null) {
				ModuleLoaded(this, new ModuleEventArgs(module));
			}
		}

		protected void OnModuleUnloaded(Module module)
		{
			if (ModuleUnloaded != null) {
				ModuleUnloaded(this, new ModuleEventArgs(module));
			}
		}

		public IList<Module> Modules { 
			get{ 
				return moduleCollection.AsReadOnly();
			} 
		}

		public Module GetModule(string filename) 
		{
			foreach(Module module in moduleCollection) {
				if (module.Filename == filename) {
					return module;
				}
			}

			throw new DebuggerException("Module \"" + filename + "\" is not in collection");
		}

		internal Module GetModule(ICorDebugModule corModule) 
		{
			foreach(Module module in moduleCollection) {
				if (module.CorModule == corModule) {
					return module;
				}
			}

			throw new DebuggerException("Module is not in collection");
		}

		internal void AddModule(Module module)
		{
			module.OrderOfLoading = lastAssignedModuleOrderOfLoading;
			lastAssignedModuleOrderOfLoading++;
			moduleCollection.Add(module);
			OnModuleLoaded(module);
		}

		internal void AddModule(ICorDebugModule corModule)
		{
			AddModule(new Module(this, corModule));
		}

		internal void RemoveModule(Module module)
		{
			moduleCollection.Remove(module);
			OnModuleUnloaded(module);
		}

		internal void RemoveModule(ICorDebugModule corModule)
		{
			RemoveModule(GetModule(corModule));
		}

		internal void ClearModules()
		{
			foreach (Module m in moduleCollection) {
 				OnModuleUnloaded(m);
				m.Dispose();
			}
			moduleCollection.Clear();
			lastAssignedModuleOrderOfLoading = 0;
		}
	}
}
