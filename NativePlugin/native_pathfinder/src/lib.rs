mod pathfinder;
mod vector3;
mod state;

use std::slice;
use std::panic;
use vector3::Vector3;
use pathfinder::{build_adjacency_list, find_path_astar};

/* ---外部呼び出し--- */
#[unsafe(no_mangle)]
pub unsafe extern "C" fn find_path(
    nodes_ptr: *const Vector3,
    node_count: i32,
    edges_ptr: *const i32,
    edge_count: i32,
    start_index: i32,
    goal_index: i32,
    out_path_ptr: *mut i32,
    max_path_length: i32
) -> i32 {
    // ポインタチェック
    if nodes_ptr.is_null() || edges_ptr.is_null() || out_path_ptr.is_null() {
        return -1;
    }

    let result = panic::catch_unwind(|| {
        let nodes = unsafe { slice::from_raw_parts(nodes_ptr, node_count as usize) };
        let raw_edges = unsafe { slice::from_raw_parts(edges_ptr, (edge_count * 3) as usize) };

        // グラフの隣接リストを作成する
        let adj_list = build_adjacency_list(raw_edges, edge_count as usize, node_count as usize);

        let start = start_index as usize;
        let goal = goal_index as usize;

        // 範囲外チェック
        if start >= nodes.len() || goal >= nodes.len() {
            return -1;
        }

        // A*アルゴリズムで経路探索
        let path_result = find_path_astar(nodes, &adj_list, start, goal);

        if let Some(path) = path_result {
            // Unityへのコピー用
            let len = path.len().min(max_path_length as usize);
            unsafe {
                for i in 0..len {
                    *out_path_ptr.add(i) = path[i] as i32;
                }
            }

            len as i32
        } else {
            -1
        }
    });

    result.unwrap_or_else(|_| -1)
}

#[cfg(test)]
mod tests {
    use super::*;

    // テスト用のヘルパー
    fn v3(x: f32, y: f32, z: f32) -> Vector3 {
        Vector3 { x, y, z }
    }

    #[test]
    fn test_simple_path() {
        // ノードを作成
        let nodes = vec![
            v3(0.0, 0.0, 0.0),
            v3(1.0, 0.0, 0.0),
            v3(2.0, 0.0, 0.0)
        ];

        // エッジの準備
        let edges = vec![
            0, 1, 1,
            1, 2, 1
        ];

        // 結果
        let mut out_path = vec![0; 10];

        let len = unsafe {
            find_path(
                nodes.as_ptr(),
                nodes.len() as i32,
                edges.as_ptr(),
                (edges.len() / 3) as i32,
                0,
                2,
                out_path.as_mut_ptr(),
                out_path.len() as i32
            )
        };

        // 検証
        assert_eq!(len, 3, "パスの長さは3が正しいです！(0, 1, 2)");
        assert_eq!(out_path[0], 0, "開始地点: 0");
        assert_eq!(out_path[1], 1, "経由地点: 1");
        assert_eq!(out_path[2], 2, "終了地点: 2");
    }

    #[test]
    fn test_obstacle_path() {
        // 障害物等を回避する
        let nodes = vec![
            v3(0.0, 0.0, 0.0),
            v3(1.0, 0.0, 0.0),
            v3(2.0, 0.0, 0.0),
            v3(1.0, 1.0, 0.0),
        ];

        let edges = vec![
            0, 1, 10,
            1, 2, 10,
            0, 3, 1,
            3, 2, 1
        ];

        let mut out_path = vec![0; 10];

        let len = unsafe {
            find_path(
                nodes.as_ptr(),
                nodes.len() as i32,
                edges.as_ptr(),
                (edges.len() / 3) as i32,
                0,
                2,
                out_path.as_mut_ptr(),
                out_path.len() as i32
            )
        };

        println!("len: {}", len);
        println!("path: {:?}", &out_path[0..len as usize]);

        assert_eq!(len, 3);
        assert_eq!(out_path[0], 0);
        assert_eq!(out_path[1], 3);
        assert_eq!(out_path[2], 2);
    }

    #[test]
    fn test_no_path() {
        // 孤立したノードへの経路探索
        let nodes = vec![
            v3(0.0, 0.0, 0.0),
            v3(10.0, 0.0, 0.0),
        ];

        let edges = vec![];

        let mut out_path = vec![0; 10];

        let len = unsafe {
            find_path(
                nodes.as_ptr(),
                nodes.len() as i32,
                edges.as_ptr(),
                0,
                0,
                1,
                out_path.as_mut_ptr(),
                out_path.len() as i32
            )
        };

        assert_eq!(len, -1, "経路がない場合は-1を返すべき");
    }
}