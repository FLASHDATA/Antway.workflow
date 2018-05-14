using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using OptimaJet.Workflow.Core.Runtime;

namespace OptimaJet.Workflow.Core.Subprocess
{
    public class SerializableSubprocessTree
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<Guid> Children { get; set; }
    }

    public class ProcessInstancesTree
    {
        public Guid Id { get; private set; }
        public string Name { get; set; }


        public ProcessInstancesTree Parent { get; private set; }

     
        public ProcessInstancesTree Root
        {
            get
            {
                if (Parent == null)
                    return this;
                return Parent.Root;
            }
        }

        public bool IsRoot
        {
            get { return Parent == null; }
        }

        public void Remove()
        {
            if (Parent != null)
            {
                ProcessInstancesTree removed;
                Parent._children.TryRemove(Id, out removed);
            }
        }

        public ProcessInstancesTree GetNodeById(Guid id, bool startFromRoot = true)
        {
            if (Id == id)
                return this;

            if (startFromRoot)
            {
                return Root.GetNodeById(id, false);
            }

            return _children.Values.Select(child => child.GetNodeById(id,false)).FirstOrDefault(res => res != null);
        }

        public List<Guid> GetAllChildrenIds(int startDistance = 1, int? endDistance = null)
        {
            CheckParametersForGetAllChildren(startDistance, endDistance);
            var res = new List<ProcessInstancesTree>();
            GetAllChildrenRecursive(res, startDistance, endDistance, 1);
            return res.Select(c=>c.Id).ToList();
        }

        public List<string> GetAllChildrenNames(int startDistance = 1, int? endDistance = null)
        {
            CheckParametersForGetAllChildren(startDistance, endDistance);
            var res = new List<ProcessInstancesTree>();
            GetAllChildrenRecursive(res, startDistance, endDistance, 1);
            return res.Select(c => c.Name).ToList();
        }

        private static void CheckParametersForGetAllChildren(int startDistance, int? endDistance)
        {
            if (startDistance < 1)
                throw new ArgumentException("startDistance must be greater or equal than 1");
            if (endDistance.HasValue && endDistance.Value < startDistance)
                throw new ArgumentException("endDistance must be greater than startDistance");
        }


        private void GetAllChildrenRecursive(List<ProcessInstancesTree> acc, int start, int? end, int current)
        {
            if (end.HasValue && current > end)
                return;

            if (current >= start)
                acc.AddRange(Children);

            foreach (var child in Children)
            {
                child.GetAllChildrenRecursive(acc, start, end, current+1);
            }
        }

        public bool HaveChildWithName(string name)
        {
            return _children.Values.Any(v => v.Name.Equals(name, StringComparison.Ordinal));
        }

      

       // public ProcessInstance Instance { get; private set; }

        public IEnumerable<ProcessInstancesTree> Children
        {
            get { return _children.Values; }
        }

        private ConcurrentDictionary<Guid,ProcessInstancesTree> _children = new ConcurrentDictionary<Guid,ProcessInstancesTree>();

        public ProcessInstancesTree(Guid id, string name = null)
        {
            Id = id;
            Name = name;
        }



        public void AddChild(ProcessInstancesTree instancesTree)
        {
            instancesTree.Parent = this;
            _children.AddOrUpdate(instancesTree.Id, instancesTree, (k, v) => v);
        }

        public void RemoveChild(ProcessInstancesTree instancesTree)
        {
            instancesTree.Parent = null;
            ProcessInstancesTree oldValue;
            _children.TryRemove(instancesTree.Id,out oldValue);
        }

        public List<dynamic> GetSerializableObject()
        {
            var root = this.Root;
            var items = new List<dynamic>();

            AddSerializableObject(root, items);

            return items;
        }

        private void AddSerializableObject(ProcessInstancesTree processTree,  List<dynamic> items)
        {
            var item = new SerializableSubprocessTree()
            {
                Id = processTree.Id,
                Name = processTree.Name,
                Children = processTree.Children.Select(c => c.Id).Distinct().ToList()
            };
            items.Add(item);

            foreach (var child in processTree.Children)
            {
                AddSerializableObject(child, items);
            }
        }

        public static ProcessInstancesTree RestoreFromSerializableObject(List<SerializableSubprocessTree> objects,
            WorkflowRuntime runtime)
        {
            var result = new Dictionary<Guid, ProcessInstancesTree>();
            foreach (var serializableObject in objects)
            {
                var processIstanceTree = new ProcessInstancesTree(serializableObject.Id, serializableObject.Name);
                result.Add(processIstanceTree.Id,processIstanceTree);
            }

            foreach (var serializableObject in objects)
            {
                if (!serializableObject.Children.Any())
                    continue;
                var processInstanceTree = result[(Guid)serializableObject.Id];

                foreach (Guid childId in serializableObject.Children)
                {
                    var child = result[childId];
                    processInstanceTree.AddChild(child);
                }
            }
            if (result.Any())
                return result.First().Value.Root;
            return null;
            
        }
    }
}
