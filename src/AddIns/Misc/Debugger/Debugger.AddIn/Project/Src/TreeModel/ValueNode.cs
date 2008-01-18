// <file>
//     <copyright license="BSD-new" see="prj:///COPYING"/>
//     <owner name="David Srbeck�" email="dsrbecky@gmail.com"/>
//     <version>$Revision$</version>
// </file>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Debugging;
using ICSharpCode.SharpDevelop.Services;

using Debugger;
using Debugger.Expressions;

namespace Debugger.AddIn.TreeModel
{
	public class ValueNode: AbstractNode, ISetText, IContextMenu
	{
		Expression expression;
		bool canSetText;
		
		public Expression Expression {
			get { return expression; }
		}
		
		public ValueNode(Value val)
		{
			this.expression = val.Expression;
			
			canSetText = false;
			if (val.IsPrimitive) {
				canSetText = 
					(val.Expression is LocalVariableIdentifierExpression) ||
					(val.Expression is ParameterIdentifierExpression) ||
					(val.Expression is ArrayIndexerExpression) ||
					(val.Expression is MemberReferenceExpression && ((MemberReferenceExpression)val.Expression).MemberInfo is FieldInfo);
			}
			
			if (val.IsObject) {
				this.Image = DebuggerIcons.ImageList.Images[0]; // Class
			} else {
				this.Image = DebuggerIcons.ImageList.Images[1]; // Field
			}
			
			this.Name = val.Expression.CodeTail;
			
			if (ShowValuesInHexadecimal && val.IsInteger) {
				this.Text = String.Format("0x{0:X}", val.PrimitiveValue);
			} else {
				this.Text =  val.AsString;
			}
			
			if (val.Type != null) {
				this.Type = val.Type.FullName;
			} else {
				this.Type = String.Empty;
			}
			
			// Note that these return enumerators so they are lazy-evaluated
			if (val.IsObject) {
				this.ChildNodes = Util.GetChildNodesOfObject(this.Expression, val.Type);
			} else if (val.IsArray) {
				this.ChildNodes = Util.GetChildNodesOfArray(this.Expression, val.ArrayDimensions);
			} else {
				this.ChildNodes = null;
			}
		}
		
		public bool CanSetText {
			get {
				return canSetText;
			}
		}
		
		public bool SetText(string newText)
		{
			Value val = null;
			try {
				val = this.Expression.Evaluate(WindowsDebugger.DebuggedProcess.SelectedStackFrame);
				val.PrimitiveValue = newText;
				this.Text = newText;
				return true;
			} catch (NotSupportedException) {
				string format = ResourceService.GetString("MainWindow.Windows.Debug.LocalVariables.CannotSetValue.BadFormat");
				string msg = String.Format(format, newText, val.Type.ManagedType.ToString());
				MessageService.ShowMessage(msg ,"${res:MainWindow.Windows.Debug.LocalVariables.CannotSetValue.Title}");
			} catch (System.Runtime.InteropServices.COMException) {
				// COMException (0x80131330): Cannot perfrom SetValue on non-leaf frames.
				// Happens if trying to set value after exception is breaked
				MessageService.ShowMessage("${res:MainWindow.Windows.Debug.LocalVariables.CannotSetValue.UnknownError}",
				                           "${res:MainWindow.Windows.Debug.LocalVariables.CannotSetValue.Title}");
			}
			return false;
		}
		
		public ContextMenuStrip GetContextMenu()
		{
			ContextMenuStrip menu = new ContextMenuStrip();
			
			ToolStripMenuItem copyItem;
			copyItem = new ToolStripMenuItem();
			copyItem.Text = ResourceService.GetString("MainWindow.Windows.Debug.LocalVariables.CopyToClipboard");
			copyItem.Checked = false;
			copyItem.Click += delegate {
				ClipboardWrapper.SetText(this.Text);
			};
			
			ToolStripMenuItem hexView;
			hexView = new ToolStripMenuItem();
			hexView.Text = ResourceService.GetString("MainWindow.Windows.Debug.LocalVariables.ShowInHexadecimal");
			hexView.Checked = ShowValuesInHexadecimal;
			hexView.Click += delegate {
				ShowValuesInHexadecimal = !ShowValuesInHexadecimal;
			};
			
			menu.Items.AddRange(new ToolStripItem[] {
			                    	copyItem,
			                    	hexView
			                    });
			
			return menu;
		}
		
		public static WindowsDebugger WindowsDebugger {
			get {
				return (WindowsDebugger)DebuggerService.CurrentDebugger;
			}
		}
		
		public static bool ShowValuesInHexadecimal {
			get {
				return WindowsDebugger.Properties.Get("ShowValuesInHexadecimal", false);
			}
			set {
				WindowsDebugger.Properties.Set("ShowValuesInHexadecimal", value);
			}
		}
	}
}
