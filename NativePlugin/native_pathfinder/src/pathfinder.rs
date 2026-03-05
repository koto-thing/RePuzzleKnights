use std::collections::BinaryHeap;
use crate::vector3::Vector3;
use crate::state::State;

/// グラフの隣接リストを構築
pub fn build_adjacency_list(
    raw_edges: &[i32],
    edge_count: usize,
    node_count: usize,
) -> Vec<Vec<(usize, f32)>> {
    let mut adj_list: Vec<Vec<(usize, f32)>> = vec![vec![]; node_count];

    for i in 0..edge_count {
        let from = raw_edges[i * 3] as usize;
        let to = raw_edges[i * 3 + 1] as usize;
        let weight = raw_edges[i * 3 + 2] as f32;

        // 範囲外チェック
        if from < node_count && to < node_count {
            adj_list[from].push((to, weight));
        }
    }

    adj_list
}

/// A*アルゴリズムで最短経路を探索
pub fn find_path_astar(
    nodes: &[Vector3],
    adj_list: &[Vec<(usize, f32)>],
    start: usize,
    goal: usize,
) -> Option<Vec<usize>> {
    let node_count = nodes.len();

    // A* アルゴリズム
    let mut dist: Vec<f32> = vec![f32::INFINITY; node_count];
    let mut heap = BinaryHeap::new();
    let mut came_from: Vec<Option<usize>> = vec![None; node_count];

    dist[start] = 0.0;
    heap.push(State { cost: 0.0, position: start });

    let mut found = false;

    while let Some(State { cost, position }) = heap.pop() {
        if position == goal {
            found = true;
            break;
        }

        // 既に処理済みのノードをスキップ
        if cost > dist[position] + nodes[position].distance(&nodes[goal]) {
            continue;
        }

        for &(neighbor, weight) in &adj_list[position] {
            let next_cost = dist[position] + weight;

            if next_cost < dist[neighbor] {
                heap.push(State {
                    cost: next_cost + nodes[neighbor].distance(&nodes[goal]),
                    position: neighbor,
                });

                dist[neighbor] = next_cost;
                came_from[neighbor] = Some(position);
            }
        }
    }

    if !found {
        return None;
    }

    // 経路を復元する
    let mut path = Vec::new();
    let mut current = goal;
    path.push(current);

    while let Some(prev) = came_from[current] {
        path.push(prev);
        current = prev;
        if prev == start {
            break;
        }
    }

    path.reverse();
    Some(path)
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::vector3::Vector3;

    fn v3(x: f32, y: f32, z: f32) -> Vector3 {
        Vector3 { x, y, z }
    }

    #[test]
    fn test_build_adjacency_list() {
        let edges = vec![0, 1, 1, 1, 2, 1];
        let adj_list = build_adjacency_list(&edges, 2, 3);

        assert_eq!(adj_list[0].len(), 1);
        assert_eq!(adj_list[0][0], (1, 1.0));
        assert_eq!(adj_list[1].len(), 1);
        assert_eq!(adj_list[1][0], (2, 1.0));
    }

    #[test]
    fn test_find_path_astar_simple() {
        let nodes = vec![
            v3(0.0, 0.0, 0.0),
            v3(1.0, 0.0, 0.0),
            v3(2.0, 0.0, 0.0),
        ];

        let mut adj_list = vec![vec![]; 3];
        adj_list[0].push((1, 1.0));
        adj_list[1].push((2, 1.0));

        let path = find_path_astar(&nodes, &adj_list, 0, 2);
        assert!(path.is_some());

        let path = path.unwrap();
        assert_eq!(path.len(), 3);
        assert_eq!(path, vec![0, 1, 2]);
    }

    #[test]
    fn test_find_path_astar_no_path() {
        let nodes = vec![
            v3(0.0, 0.0, 0.0),
            v3(10.0, 0.0, 0.0),
        ];

        let adj_list = vec![vec![]; 2];

        let path = find_path_astar(&nodes, &adj_list, 0, 1);
        assert!(path.is_none());
    }
}
