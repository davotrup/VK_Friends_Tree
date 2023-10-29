``` python
import numpy as np

class Graph:
    def __init__(self, num_nodes):
        self.num_nodes = num_nodes
        self.adj_matrix = [[0] * num_nodes for _ in range(num_nodes)]

    def add_edge(self, node1, node2):
        if node1 < 0 or node1 >= self.num_nodes or node2 < 0 or node2 >= self.num_nodes:
            raise ValueError("One or both nodes are out of range.")
        self.adj_matrix[node1][node2] = 1
        self.adj_matrix[node2][node1] = 1

    def get_shortest_paths(self, node):
        queue = [(node, [])]
        visited = set()
        paths = {}

        while queue:
            current_node, current_path = queue.pop(0)
            visited.add(current_node)

            if current_node not in paths:
                paths[current_node] = []

            if current_path not in paths[current_node]:
                paths[current_node].append(current_path)

            neighbors = [i for i in range(self.num_nodes) if self.adj_matrix[current_node][i] == 1]
            for neighbor in neighbors:
                if neighbor not in visited:
                    queue.append((neighbor, current_path + [neighbor]))

        return paths

    def betweenness_centrality(self):
        betweenness = {node: 0 for node in range(self.num_nodes)}

        for node in range(self.num_nodes):
            paths = self.get_shortest_paths(node)
            total_paths = len(paths)

            for source in paths:
                for target in paths:
                    if source != target:
                        for path in paths[source]:
                            if target in path:
                                betweenness[node] += 1 / total_paths

        return betweenness

    def closeness_centrality(self):
        closeness = {}

        for node in range(self.num_nodes):
            total_distance = 0
            paths = self.get_shortest_paths(node)

            for target in paths:
                if target != node:
                    total_distance += len(paths[target][0]) - 1

            closeness[node] = (len(paths) - 1) / total_distance if total_distance > 0 else 0

        return closeness# Вычисление центральности по собственному значению

    def eigenvector_centrality(self):
        eigenvalues, eigenvectors = np.linalg.eig(self.adj_matrix)
        dominant_eigenvector = eigenvectors[:, np.argmax(eigenvalues)]
        centrality = dominant_eigenvector / np.sum(dominant_eigenvector)
        return centrality


# Создание примера графа
graph = Graph(5)
graph.add_edge(0, 1)
graph.add_edge(0, 2)
graph.add_edge(1, 2)
graph.add_edge(2, 3)
graph.add_edge(3, 4)

# Центральность посредничества (betweenness centrality)
betweenness_centralities = graph.betweenness_centrality()
print("Betweenness Centrality:")
for node, centrality in betweenness_centralities.items():
    print(f"Node {node}: {centrality}")

# Центральность близости (closeness centrality)
closeness_centralities = graph.closeness_centrality()
print("Closeness Centrality:")
for node, centrality in closeness_centralities.items():
    print(f"Node {node}: {centrality}")

print("Eigenvector Centrality:")
centrality = graph.eigenvector_centrality()
# Вывод значений центральности
for node, value in enumerate(centrality, start=0):
    print(f"Node {node}: {value}")
```