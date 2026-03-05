/// 3次元ベクトル構造体（C互換）
#[repr(C)]
#[derive(Clone, Copy, Debug)]
pub struct Vector3 {
    pub x: f32,
    pub y: f32,
    pub z: f32,
}

impl Vector3 {
    /// 2点間のユークリッド距離を計算
    pub fn distance(&self, other: &Vector3) -> f32 {
        ((self.x - other.x).powi(2) + (self.y - other.y).powi(2) + (self.z - other.z).powi(2)).sqrt()
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_distance() {
        let v1 = Vector3 { x: 0.0, y: 0.0, z: 0.0 };
        let v2 = Vector3 { x: 3.0, y: 4.0, z: 0.0 };
        assert_eq!(v1.distance(&v2), 5.0);
    }
}
