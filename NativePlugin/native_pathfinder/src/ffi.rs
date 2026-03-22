use std::slice;
use std::panic;
use crate::vector3::Vector3;
use crate::pathfinder::{build_adjacency_list, find_path_astar};

/// C FFIインターフェース: A*アルゴリズムで経路を探索
///
/// # Safety
/// - `nodes_ptr`は有効なVector3の配列へのポインタである必要があります
/// - `edges_ptr`は有効なi32の配列へのポインタである必要があります
/// - `out_path_ptr`は有効な書き込み可能なi32配列へのポインタである必要があります
/// - ポインタはNULLではなく、正しくアライされている必要があります
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
        // Safety: ポインタはNULLチェック済み、サイズは呼び出し元が保証
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
            // 結果をC側に書き込む
            let len = path.len().min(max_path_length as usize);

            // Safety: out_path_ptrはNULLチェック済み、アライメントと有効性は呼び出し元が保証
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

