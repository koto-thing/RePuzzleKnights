void CalculateSlash_float(
float2 UV,
float2 Center,
float Angle,
float Separation,
float EdgeWidth,
out float2 OutUV,
out float EdgeMask,
out float CenterMask
) {
    // UVを中心基準に変換する
    float2 p = UV - Center;
    
    // 角度をラジアンに変換
    float rad = radians(Angle);
    float c = cos(rad);
    float s = sin(rad);
    
    // 回転行列
    float2x2 rotationMat = float2x2(c, -s, s, c);
    float2x2 invRotationMat = float2x2(c, s, -s, c);
    
    // UVを回転
    float2 p_rotated = mul(rotationMat, p);
    
    // 領域を分離
    float side = sign(p_rotated.x);
    
    // Taper
    float taper = cos(clamp(p_rotated.y, -0.5, 0.5) * 3.14159);
    taper = max(0.0, taper);

    // 空間のズレ
    p_rotated.y += side * Separation * 0.5 * taper;
    
    // 歪み
    p_rotated.x -= side * smoothstep(0.0, 0.2, abs(p_rotated.x)) * Separation * 0.1 * taper;
    
    // 中抜きの形状を作成
    float targetX = EdgeWidth * 0.3 * taper;
    
    // 線の太さを定義
    float lineThickness = (EdgeWidth * 0.2) * taper;
    float distFromLine = abs(abs(p_rotated.x) - targetX);
    EdgeMask = smoothstep(lineThickness, 0.0, distFromLine);
    CenterMask = smoothstep(targetX, 0.0, abs(p_rotated.x));
    
    // 逆回転させて元の座標系に戻す
    float2 p_final = mul(invRotationMat, p_rotated);
    
    // 中心位置を戻して出力UVにしておく
    OutUV = p_final + Center;
}