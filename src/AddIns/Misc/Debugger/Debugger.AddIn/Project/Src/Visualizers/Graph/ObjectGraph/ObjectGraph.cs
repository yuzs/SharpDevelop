﻿// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Martin Koníček" email="martin.konicek@gmail.com"/>
//     <version>$Revision$</version>
// </file>
using System;
using System.Collections.Generic;
using System.Text;

namespace Debugger.AddIn.Visualizers.Graph
{
	/// <summary>
	/// Object graph built by <see cref="ObjectGraphBuilder"/>
	/// </summary>
    public class ObjectGraph
    {
    	/// <summary>
    	/// Root of the graph.
    	/// </summary>
    	public ObjectNode Root { get; internal set; }
    	
    	/// <summary>
    	/// Adds node to the graph.
    	/// </summary>
    	/// <param name="node">node to be added</param>
    	internal void AddNode(ObjectNode node)
    	{
    		_nodes.Add(node);
    	}
    	
    	private List<ObjectNode> _nodes = new List<ObjectNode>();
        /// <summary>
        /// All nodes in the graph.
        /// </summary>
        public IEnumerable<ObjectNode> Nodes 
        {
            get { return _nodes; }
        }
        
        /// <summary>
        /// All edges in the graph.
        /// </summary>
        /*public IEnumerable<ObjectEdge> Edges
        {
            get 
            {
            	foreach	(ObjectNode node in this.Nodes)
            	{
            		foreach (ObjectEdge edge in node.Edges)
            		{
            			yield return edge;
            		}
            	}
        	}
        }*/
    }
}
