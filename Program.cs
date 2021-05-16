using System;
using System.Collections.Generic;
using System.IO;

namespace RiverCrossingSolver {
    class Program {

        static List<Node> nodes = new List<Node>();
        static List<Edge> edges = new List<Edge>();
        static Dictionary<Node, Dictionary<Node, int>> edgeFetch = new Dictionary<Node, Dictionary<Node, int>>();

        static void Main(string[] args) {
            int k = int.Parse(args[0]);
            string fileOutput = args[1];
            //int k = 0;
            //string fileOutput = "";

            // Get valid nodes
            for (int i = 0; i < Math.Pow(2, 4); i++) {
                var node = new Node(GetBit(i, 0), GetBit(i, 1), GetBit(i, 2), GetBit(i, 3));

                if (node.IsValid) {
                    nodes.Add(node);
                }
            }

            // Get valid edges
            foreach (var u in nodes) {
                foreach (var v in nodes) {
                    if (u == v) continue;
                    if (!CanConnect(u, v))
                        continue;
                    AddEdge(u, v);
                }
            }

            int variables = nodes.Count + k * edges.Count;
            int clauses = 0;
            string output = "";

            // start and end
            output += $"1 0\n{nodes.Count} 0\n";
            clauses += 2;

            // only one each step
            for (int t = 0; t < k; t++) {
                foreach (var e1 in edges) {
                    foreach (var e2 in edges) {
                        if (e1 == e2) continue;
                        output += $"{-GetEdge(t, e1.a, e1.b)} {-GetEdge(t, e2.a, e2.b)} 0\n";
                        clauses++;
                    }
                }
            }

            // highest step has at least one
            string temp = "";
            foreach (var edge in edges) {
                temp += $" {GetEdge(k - 1, edge.a, edge.b)}";
            }
            output += temp.Trim() + " 0\n";
            clauses++;

            // edge's both nodes are included
            for (int t = 0; t < k; t++) {
                foreach (var edge in edges) {
                    // left side
                    output += $"{-GetEdge(t, edge.a, edge.b)} {nodes.IndexOf(edge.a) + 1} 0\n";
                    clauses++;

                    // right side
                    output += $"{-GetEdge(t, edge.a, edge.b)} {nodes.IndexOf(edge.b) + 1} 0\n";
                    clauses++;
                }
            }

            // adjacent edges have a connecting node
            for (int t = 1; t < k; t++) {
                foreach (var n3 in nodes) {
                    foreach (var n2 in nodes) {
                        if (!edgeFetch[n2].ContainsKey(n3))
                            continue;
                        output += $"{-GetEdge(t, n2, n3)}";

                        foreach (var n1 in nodes) {
                            if (!edgeFetch[n1].ContainsKey(n2))
                                continue;
                            output += $" {GetEdge(t - 1, n1, n2)}";
                        }

                        output += " 0 \n";
                        clauses++;
                    }
                }
            }

            // node always has a connecting edge
            foreach (var node in nodes) {
                output += $"{-(nodes.IndexOf(node) + 1)}";

                for (int t = 0; t < k; t++) {
                    foreach (var n2 in nodes) {
                        if (!edgeFetch[node].ContainsKey(n2))
                            continue;
                        output += $" {GetEdge(t, node, n2)}";
                    }
                }

                output += " 0 \n";
                clauses++;
            }

            output = $"p cnf {variables} {clauses}" + "\n" + output;
            File.WriteAllText(fileOutput, output);
        }

        static void AddEdge(Node a, Node b) {
            if (!edges.Contains(new Edge(b, a)))
                edges.Add(new Edge(a, b));
            if (!edgeFetch.ContainsKey(a))
                edgeFetch.Add(a, new Dictionary<Node, int>());
            if (!edgeFetch[a].ContainsKey(b))
                edgeFetch[a].Add(b, 0);
            edgeFetch[a][b] = edges.Count;
        }

        static int GetEdge(int t, Node a, Node b) {
            return nodes.Count + t * edges.Count + edgeFetch[a][b];
        }

        static bool CanConnect(Node a, Node b) {
            // boat has to move
            if (a.boat == b.boat) return false;
            // keep track if we are carrying something already
            bool carry = false;

            // was wolf moved?
            if (a.wolf != b.wolf) {
                // moved object must move in the direction of the boat
                if (b.wolf != b.boat) return false;
                carry = true;
            }

            // was rabbit moved?
            if (a.rabbit != b.rabbit) {
                // can't move multiple things
                if (carry) return false;
                if (b.rabbit != b.boat) return false;
                carry = true;
            }

            // was carrot moved?
            if (a.carrot != b.carrot) {
                if (carry) return false;
                if (b.carrot != b.boat) return false;
            }

            return true;
        }

        static bool GetBit(int number, int nth_bit) {
            return number != 0 && (number & (1 << nth_bit)) != 0;
        }
    }

    struct Edge {
        public Node a;
        public Node b;

        public Edge(Node a, Node b) {
            this.a = a;
            this.b = b;
        }

        public static bool operator ==(Edge a, Edge b) => a.a == b.a && a.b == b.b;
        public static bool operator !=(Edge a, Edge b) => !(a == b);
        public override bool Equals(object obj) => obj is Edge edge && edge == this;
        public override int GetHashCode() => HashCode.Combine(a, b);
    }

    struct Node {
        public bool wolf;
        public bool rabbit;
        public bool carrot;
        public bool boat;

        public bool IsValid => !((wolf == rabbit && wolf != boat) || (rabbit == carrot && rabbit != boat));

        public Node(bool wolf, bool rabbit, bool carrot, bool boat) {
            this.wolf = wolf;
            this.rabbit = rabbit;
            this.carrot = carrot;
            this.boat = boat;
        }

        public static bool operator ==(Node a, Node b) => a.wolf == b.wolf && a.rabbit == b.rabbit && a.carrot == b.carrot && a.boat == b.boat;
        public static bool operator !=(Node a, Node b) => !(a == b);
        public override bool Equals(object obj) => obj is Node node && node == this;
        public override int GetHashCode() => HashCode.Combine(wolf, rabbit, carrot, boat);
        public override string ToString() => (wolf ? "1" : "0") + (rabbit ? "1" : "0") + (carrot ? "1" : "0") + (boat ? "1" : "0");
    }
}
