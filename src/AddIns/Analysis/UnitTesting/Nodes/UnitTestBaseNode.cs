﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Linq;
using ICSharpCode.TreeView;

namespace ICSharpCode.UnitTesting
{
	public abstract class UnitTestBaseNode : SharpTreeNode
	{
		internal abstract TestResultType TestResultType { get; }
		
		public override object Icon {
			get { return GetIcon(TestResultType); }
		}
		
		object GetIcon(TestResultType testResultType)
		{
			switch (testResultType) {
				case TestResultType.None:
					return Images.Grey;
				case TestResultType.Success:
					return Images.Green;
				case TestResultType.Failure:
					return Images.Red;
				case TestResultType.Ignored:
					return Images.Yellow;
				default:
					throw new Exception("Invalid value for TestResultType");
			}
		}
	}
}
