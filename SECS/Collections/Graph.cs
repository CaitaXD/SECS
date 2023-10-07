using System.Collections;
using System.Runtime.CompilerServices;
using CommunityToolkit.HighPerformance;

namespace SECS.Collections;
public sealed class DirectedAcyclicGraph<TVertex, TEdge>
{
    public IEqualityComparer<TVertex> VertexComparer { get; init; }
    public IEqualityComparer<TEdge> EdgeComparer { get; init; }

    public GraphNode Root { get; }

    TVertex[]   _vertices;
    GraphEdge[] _edges;

    const int DefaultCapacity = 1;
    const int GrowFactor      = 2;

    Span2D<GraphEdge> AdjacencyMatrix => new(_edges, _vertices.Length, _vertices.Length);

    int Count { get; set; }

    public DirectedAcyclicGraph(TVertex root)
    {
        VertexComparer = EqualityComparer<TVertex>.Default;
        EdgeComparer = EqualityComparer<TEdge>.Default;
        _vertices = new TVertex[DefaultCapacity];
        _edges = new GraphEdge[DefaultCapacity * DefaultCapacity];
        Root = new GraphNode(this, root);
        _vertices[0] = root;
        Count = 1;
    }
    public void Grow(int factor = GrowFactor, int min = DefaultCapacity)
    {
        var capacity = Math.Max(_vertices.Length * factor, min);
        Array.Resize(ref _vertices, capacity);
        Array.Resize(ref _edges, capacity * capacity);
    }
    public void EnsureCapacity(int count, int factor = GrowFactor)
    {
        if (count >= _vertices.Length) {
            Grow(factor, count);
        }
    }
    public void Add(TVertex from, TEdge value, TVertex to)
    {
        EnsureCapacity(Count + 2);

        int idxFrom = Array.IndexOf(_vertices, from);
        if (idxFrom < 0) {
            idxFrom = Count;
            _vertices[idxFrom] = from;
            Count++;
        }
        int idxTo = Array.IndexOf(_vertices, to);
        if (idxTo < 0) {
            idxTo = Count;
            _vertices[idxTo] = to;
            Count++;
        }
        AdjacencyMatrix[idxFrom, idxTo] = new GraphEdge(Direction.Forward, value);
    }
    public bool Remove(TVertex from, TVertex to)
    {
        int idxFrom = Array.IndexOf(_vertices, from);
        if (idxFrom < 0) {
            return false;
        }
        int idxTo = Array.IndexOf(_vertices, to);
        if (idxTo < 0) {
            return false;
        }
        AdjacencyMatrix[idxFrom, idxTo] = default!;
        return true;
    }
    public enum Direction
    {
        None,
        Forward,
        Backward
    }
    public readonly record struct GraphEdge(Direction Direction, TEdge Value);
    public readonly struct GraphNode
    {
        public DirectedAcyclicGraph<TVertex, TEdge> AcyclicGraph { get; }
        public TVertex Value { get; }

        public GraphNode(DirectedAcyclicGraph<TVertex, TEdge> acyclicGraph, TVertex value)
        {
            Value = value;
            AcyclicGraph = acyclicGraph;
        }

        public bool TryMoveNext(TEdge edge, out GraphNode node)
        {
            var idx = Array.IndexOf(AcyclicGraph._vertices, Value);
            for (int i = 0; i < AcyclicGraph.Count; i++) {
                var tEdge = AcyclicGraph.AdjacencyMatrix[idx, i];
                if (tEdge.Direction is Direction.Forward && AcyclicGraph.EdgeComparer.Equals(tEdge.Value, edge)) {
                    node = new GraphNode(AcyclicGraph, AcyclicGraph._vertices[i]);
                    return true;
                }
            }
            node = default;
            return false;
        }

        public GraphNode Next(TEdge edge)
        {
            var idx = Array.IndexOf(AcyclicGraph._vertices, Value);
            for (int i = 0; i < AcyclicGraph.Count; i++) {
                var tEdge = AcyclicGraph.AdjacencyMatrix[idx, i];
                if (tEdge.Direction is Direction.Forward && AcyclicGraph.EdgeComparer.Equals(tEdge.Value, edge)) {
                    return new GraphNode(AcyclicGraph, AcyclicGraph._vertices[i]);
                }
            }
            throw new InvalidOperationException("Edge does not exist");
        }

        public bool DepthFirstSearch(TVertex vertex, out GraphNode graphNode)
        {
            BitArray visited = new BitArray(AcyclicGraph.Count);
            var      stack   = new Stack<GraphNode>();
            int      idx     = Array.IndexOf(AcyclicGraph._vertices, Value);

            stack.Push(this);
            visited[idx] = true;

            while (stack.Count > 0) {
                var node = stack.Pop();

                if (AcyclicGraph.VertexComparer.Equals(node.Value, vertex)) {
                    graphNode = node;
                    return true;
                }
                var edges = AcyclicGraph.AdjacencyMatrix.GetRowSpan(idx);
                foreach (var edge in edges) {
                    if (!node.TryMoveNext(edge.Value, out var neighbor)) {
                        continue;
                    }
                    idx = Array.IndexOf(AcyclicGraph._vertices, neighbor.Value);
                    if (!visited[idx]) {
                        stack.Push(neighbor);
                        visited[idx] = true;
                    }
                }
            }
            graphNode = default;
            return false;
        }
        public DepthFirstEnumerator DepthFirstEnumerator =>
            new(ref Unsafe.AsRef(this), new BitArray(AcyclicGraph.Count), new Stack<GraphNode>());
    }
    public ref struct DepthFirstEnumerator
    {
        public ref GraphNode Current => ref _current;

        readonly ref GraphNode        _current;
        readonly     BitArray         _visited;
        readonly     Stack<GraphNode> _stack;
        public DepthFirstEnumerator(ref GraphNode root, BitArray visited, Stack<GraphNode> stack)
        {
            _current = ref root;
            _visited = visited;
            _stack = stack;
        }

        int _index = -1;
        public bool MoveNext()
        {
            if (_index == -1) {
                _stack.Push(Current);
                _index = Array.IndexOf(Current.AcyclicGraph._vertices, Current.Value);
                _visited[_index] = true;
            }
            if (_stack.Count == 0) {
                return false;
            }
            Current = _stack.Pop();
            var edges = Current.AcyclicGraph.AdjacencyMatrix.GetRowSpan(_index);
            foreach (var edge in edges) {
                if (!Current.TryMoveNext(edge.Value, out var neighbor)) {
                    continue;
                }
                _index = Array.IndexOf(Current.AcyclicGraph._vertices, neighbor.Value);
                if (!_visited[_index]) {
                    _stack.Push(neighbor);
                    _visited[_index] = true;
                }
            }
            return true;
        }

        public DepthFirstEnumerator GetEnumerator() => this;
    }
}