using System.Collections.Generic;
using ChronosAndCards.Interfaces;

namespace ChronosAndCards.Data
{
    /// <summary>
    /// Estructura de datos de Grafo Dirigido Acíclico (DAG) que representa
    /// las casillas y sus conexiones/bifurcaciones en el modo de Exploración.
    /// </summary>
    public class BoardGraph
    {
        private readonly Dictionary<ITile, List<ITile>> _adjacencyList;

        /// <summary>Nodo de inicio del tablero.</summary>
        public ITile StartNode { get; }

        /// <summary>Nodo final (Meta) del tablero.</summary>
        public ITile EndNode { get; }

        /// <summary>
        /// Constructor que inicializa el grafo con los nodos de inicio y fin.
        /// </summary>
        public BoardGraph(ITile startNode, ITile endNode)
        {
            StartNode = startNode;
            EndNode = endNode;
            _adjacencyList = new Dictionary<ITile, List<ITile>>();
            
            // Registrar nodos principales en la lista de adyacencia
            _adjacencyList[StartNode] = new List<ITile>();
            _adjacencyList[EndNode] = new List<ITile>();
        }

        /// <summary>
        /// Añade una conexión dirigida desde una casilla origen hacia una casilla destino.
        /// </summary>
        public void AddEdge(ITile from, ITile to)
        {
            if (!_adjacencyList.ContainsKey(from))
            {
                _adjacencyList[from] = new List<ITile>();
            }
            if (!_adjacencyList.ContainsKey(to))
            {
                _adjacencyList[to] = new List<ITile>();
            }

            if (!_adjacencyList[from].Contains(to))
            {
                _adjacencyList[from].Add(to);
            }
        }

        /// <summary>
        /// Retorna la lista de casillas vecinas de salida accesibles desde el nodo dado.
        /// </summary>
        public IReadOnlyList<ITile> GetNeighbors(ITile node)
        {
            if (node != null && _adjacencyList.TryGetValue(node, out List<ITile> neighbors))
            {
                return neighbors.AsReadOnly();
            }
            return new List<ITile>().AsReadOnly();
        }

        /// <summary>
        /// Retorna todas las casillas únicas que forman parte del grafo.
        /// </summary>
        public IReadOnlyCollection<ITile> GetAllNodes()
        {
            return _adjacencyList.Keys;
        }

        /// <summary>
        /// Retorna la cantidad de conexiones de salida de un nodo.
        /// </summary>
        public int GetOutDegree(ITile node)
        {
            if (node != null && _adjacencyList.TryGetValue(node, out List<ITile> neighbors))
            {
                return neighbors.Count;
            }
            return 0;
        }

        /// <summary>
        /// Verifica mediante DFS/BFS si existe al menos una ruta continua desde el nodo StartNode hasta el EndNode.
        /// </summary>
        public bool HasPathToEnd()
        {
            if (StartNode == null || EndNode == null) return false;

            var visited = new HashSet<ITile>();
            var queue = new Queue<ITile>();

            queue.Enqueue(StartNode);
            visited.Add(StartNode);

            while (queue.Count > 0)
            {
                ITile current = queue.Dequeue();

                if (current == EndNode)
                {
                    return true;
                }

                foreach (ITile neighbor in GetNeighbors(current))
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return false;
        }
    }
}
