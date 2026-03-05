use std::cmp::Ordering;

/// A*アルゴリズムで使用する優先度付きキューの要素
#[derive(Copy, Clone, PartialEq)]
pub struct State {
    pub cost: f32,
    pub position: usize, // ノードのインデックス
}

impl Eq for State {}

impl Ord for State {
    /// 逆順にすることで、BinaryHeapが最小値を優先するようにする
    fn cmp(&self, other: &Self) -> Ordering {
        other.cost.total_cmp(&self.cost)
    }
}

impl PartialOrd for State {
    fn partial_cmp(&self, other: &Self) -> Option<Ordering> {
        Some(self.cmp(other))
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::collections::BinaryHeap;

    #[test]
    fn test_state_ordering() {
        let mut heap = BinaryHeap::new();
        heap.push(State { cost: 5.0, position: 0 });
        heap.push(State { cost: 1.0, position: 1 });
        heap.push(State { cost: 3.0, position: 2 });

        // 最小値が先に取り出されるべき
        let first = heap.pop().unwrap();
        assert_eq!(first.cost, 1.0);
    }
}
