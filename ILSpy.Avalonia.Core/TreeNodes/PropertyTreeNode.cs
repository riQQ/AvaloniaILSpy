﻿// Copyright (c) 2011 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Reflection;
using System.Reflection.Metadata;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using SRM = System.Reflection.Metadata;

namespace ICSharpCode.ILSpy.TreeNodes
{
	/// <summary>
	/// Represents a property in the TreeView.
	/// </summary>
	public sealed class PropertyTreeNode : ILSpyTreeNode, IMemberTreeNode
	{
		readonly bool isIndexer;

		public PropertyTreeNode(IProperty property)
		{
			this.PropertyDefinition = property ?? throw new ArgumentNullException(nameof(property));
			this.isIndexer = property.IsIndexer;

			if (property.CanGet)
				this.Children.Add(new MethodTreeNode(property.Getter));
			if (property.CanSet)
				this.Children.Add(new MethodTreeNode(property.Setter));
			/*foreach (var m in property.OtherMethods)
				this.Children.Add(new MethodTreeNode(m));*/
		}

		public IProperty PropertyDefinition { get; }

		public override object Text => GetText(PropertyDefinition, Language) + PropertyDefinition.MetadataToken.ToSuffixString();

		public static object GetText(IProperty property, Language language)
		{
			return language.PropertyToString(property, false, false, false);
		}

		public override object Icon => GetIcon(PropertyDefinition);

		public static IBitmap GetIcon(IProperty property)
		{
			return Images.GetIcon(property.IsIndexer ? MemberIcon.Indexer : MemberIcon.Property,
				MethodTreeNode.GetOverlayIcon(property.Accessibility), property.IsStatic);
		}

		public override FilterResult Filter(FilterSettings settings)
		{
			if (!settings.ShowInternalApi && !IsPublicAPI)
				return FilterResult.Hidden;
			if (settings.SearchTermMatches(PropertyDefinition.Name) && settings.Language.ShowMember(PropertyDefinition))
				return FilterResult.Match;
			else
				return FilterResult.Hidden;
		}

		public override void Decompile(Language language, ITextOutput output, DecompilationOptions options)
		{
			language.DecompileProperty(PropertyDefinition, output, options);
		}

		public override bool IsPublicAPI {
			get {
				switch (PropertyDefinition.Accessibility) {
					case Accessibility.Public:
					case Accessibility.ProtectedOrInternal:
					case Accessibility.Protected:
						return true;
					default:
						return false;
				}
			}
		}

		IEntity IMemberTreeNode.Member => PropertyDefinition;
	}
}
