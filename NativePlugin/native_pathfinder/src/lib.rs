mod pathfinder;
mod vector3;
mod state;
mod ffi;

pub use ffi::find_path;

#[cfg(test)]
mod tests {
    use crate::vector3::Vector3;
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