using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NKD.Helpers
{
    public class TreeHelper<T>
    {
        
        private TreeNode<T> _root;
        public TreeNode<T> Root { get {return _root;} }
        public TreeNode<T> LastNode { get; set; }

        public TreeHelper()
        {
            _root = null;
            LastNode = null;
        }

        public void AddChild(TreeNode<T> node, TreeNode<T> destination=null)
        {
            if (_root == null)
            {
                _root = node;
                LastNode = node;
            }
            else
            {
                if (destination == null)
                    destination = LastNode;
                if (destination.Child != null)
                {
                    AddSibling(node, LastNode.Child);
                }
                else
                {
                    destination.Child = node;
                    LastNode = node;
                }
            }
        }

        public void AddSibling(TreeNode<T> node, TreeNode<T> destination=null)
        {
            if (_root == null)
            {
                AddChild(node);
                return;
            }
            if (destination == null)
                destination = LastNode;
            if (destination.Sibling != null)
                AddSibling(node, destination.Sibling);
            else
            {
                destination.Sibling = node;
                LastNode = node;
            }
        }

        public TreeNode<T> FindNode(T data, TreeNode<T> searchRoot=null)
        {
			if(_root == null) {
                return null;				
			}
            if (searchRoot == null)
                searchRoot = _root;
            if (data.ComputeHash() == searchRoot.Data.ComputeHash()) //TODO Slow but ok for now
                return searchRoot;
            if (searchRoot.Child != null)
                return FindNode(data, searchRoot.Child);
            if (searchRoot.Sibling != null)
                return FindNode(data, searchRoot.Sibling);
            return null;
        }
        
    }


    public class TreeNode<T>
    {
        public T Data { get; set; }
        public TreeNode<T> Child { get; set; }
        public TreeNode<T> Sibling { get; set; }

        public TreeNode(T data)
        {
            Data = data;
        }

    }

    class TreeNode
    {
        public static TreeNode<T> ctor<T>(T x) { return new TreeNode<T>(x); }
    }
}