using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(Image), true)]
[CanEditMultipleObjects]
public class ImageColorEditor : Editor
{
    // 색상 팔레트 참조
    private ColorPalette colorPalette;
    private SerializedProperty colorProperty;

    // 설정 파일 경로
    private const string ColorPalettePath = "Assets/@Resources/ColorPalette.asset";

    private void OnEnable()
    {
        // 색상 프로퍼티 가져오기
        colorProperty = serializedObject.FindProperty("m_Color");
        
        // 색상 팔레트 로드
        colorPalette = AssetDatabase.LoadAssetAtPath<ColorPalette>(ColorPalettePath);
        
        // 팔레트가 없으면 경고 표시
        if (colorPalette == null)
        {
            Debug.LogWarning("색상 팔레트를 찾을 수 없습니다. 경로를 확인하세요: " + ColorPalettePath);
        }
    }

    public override void OnInspectorGUI()
    {
        // 기본 인스펙터 그리기
        base.OnInspectorGUI();
        
        serializedObject.Update();
        
        // 구분선 추가
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("색상 팔레트", EditorStyles.boldLabel);
        
        if (colorPalette != null && colorPalette.colors.Count > 0)
        {
            EditorGUILayout.BeginHorizontal();
            
            // 색상 버튼 그리드로 표시
            int columns = 4;
            int index = 0;
            
            foreach (var colorItem in colorPalette.colors)
            {
                if (index % columns == 0 && index > 0)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }
                
                // 색상 버튼 생성
                GUI.backgroundColor = colorItem.color;
                
                if (GUILayout.Button(new GUIContent("", colorItem.colorName), GUILayout.Width(30), GUILayout.Height(30)))
                {
                    Undo.RecordObject(target, "Change Image Color");
                    colorProperty.colorValue = colorItem.color;
                    serializedObject.ApplyModifiedProperties();
                }
                
                index++;
            }
            
            EditorGUILayout.EndHorizontal();
            
            // 현재 선택된 색상 정보 표시
            EditorGUILayout.Space();
            
            // 툴팁에 색상 이름 표시
            EditorGUI.BeginDisabledGroup(true);
            GUI.backgroundColor = Color.white;
            
            string selectedColorName = "사용자 정의";
            foreach (var colorItem in colorPalette.colors)
            {
                if (colorProperty.colorValue == colorItem.color)
                {
                    selectedColorName = colorItem.colorName;
                    break;
                }
            }
            
            EditorGUILayout.TextField("선택된 색상", selectedColorName);
            EditorGUI.EndDisabledGroup();
        }
        else
        {
            EditorGUILayout.HelpBox("색상 팔레트가 비어있거나 로드되지 않았습니다.", MessageType.Warning);
            
            if (GUILayout.Button("색상 팔레트 생성"))
            {
                CreateColorPalette();
            }
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void CreateColorPalette()
    {
        // 폴더 생성
        if (!System.IO.Directory.Exists("Assets/@Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "@Resources");
        }
        
        // 새 팔레트 생성
        ColorPalette newPalette = ScriptableObject.CreateInstance<ColorPalette>();
        
        // 기본 색상 추가
        // 웹/앱 디자인에 많이 사용되는 현대적 색상들
        newPalette.colors.Add(new ColorPalette.ColorItem { colorName = "코발트 블루", color = new Color(0.0f, 0.28f, 0.67f) });
        newPalette.colors.Add(new ColorPalette.ColorItem { colorName = "티파니 블루", color = new Color(0.13f, 0.69f, 0.67f) });
        newPalette.colors.Add(new ColorPalette.ColorItem { colorName = "퍼시픽 블루", color = new Color(0.11f, 0.47f, 0.73f) });
        newPalette.colors.Add(new ColorPalette.ColorItem { colorName = "에메랄드 그린", color = new Color(0.0f, 0.62f, 0.38f) });
        newPalette.colors.Add(new ColorPalette.ColorItem { colorName = "민트", color = new Color(0.56f, 0.93f, 0.76f) });
        newPalette.colors.Add(new ColorPalette.ColorItem { colorName = "세이지 그린", color = new Color(0.53f, 0.6f, 0.42f) });
        newPalette.colors.Add(new ColorPalette.ColorItem { colorName = "코랄 핑크", color = new Color(1.0f, 0.5f, 0.5f) });
        newPalette.colors.Add(new ColorPalette.ColorItem { colorName = "라벤더", color = new Color(0.71f, 0.47f, 0.87f) });
        newPalette.colors.Add(new ColorPalette.ColorItem { colorName = "머스타드 옐로우", color = new Color(0.97f, 0.78f, 0.22f) });
        newPalette.colors.Add(new ColorPalette.ColorItem { colorName = "버건디", color = new Color(0.5f, 0.0f, 0.13f) });
        newPalette.colors.Add(new ColorPalette.ColorItem { colorName = "테라코타", color = new Color(0.89f, 0.45f, 0.36f) });
        newPalette.colors.Add(new ColorPalette.ColorItem { colorName = "피치", color = new Color(1.0f, 0.85f, 0.73f) });
        newPalette.colors.Add(new ColorPalette.ColorItem { colorName = "턱웨도 네이비", color = new Color(0.13f, 0.2f, 0.36f) });
        newPalette.colors.Add(new ColorPalette.ColorItem { colorName = "슬레이트 그레이", color = new Color(0.44f, 0.5f, 0.56f) });
        newPalette.colors.Add(new ColorPalette.ColorItem { colorName = "마젠타", color = new Color(0.86f, 0.15f, 0.5f) });
        
        // 필요시 기본색 추가
        newPalette.colors.Add(new ColorPalette.ColorItem { colorName = "블랙", color = Color.black });
        newPalette.colors.Add(new ColorPalette.ColorItem { colorName = "화이트", color = Color.white });
        
        // 에셋 생성
        AssetDatabase.CreateAsset(newPalette, ColorPalettePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // 새 팔레트 로드
        colorPalette = newPalette;
    }

}