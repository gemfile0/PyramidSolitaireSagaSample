# Pyramid Solitaire Saga Sample
King.com 의 Pyramid Solitaire Saga 게임을 따라 만든 샘플 Unity 프로젝트입니다.

- [WebGL 빌드 플레이 링크 (itch.io)](https://gemfile0.itch.io/pyramid-solitaire-saga-made-with-unity?secret=CfcK7JLitcgcBErwJd38efX57gY)
- [11레벨 디자인 후 직접 플레이하는 영상 링크 (YouTube)](https://youtu.be/FUR7Q9k_uoo)  
- [50레벨 디자인 후 직접 플레이하는 영상 링크 (YouTube)](https://youtu.be/xlwPmFhutOU)
  > [레퍼런스 게임 플레이 영상 링크 (YouTube)](https://youtu.be/YH51ldCczJ8)

## 게임 방법
![image](https://github.com/gemfile0/PyramidSolitaireSagaSample/assets/369285/9f5c570e-c0d3-468a-9fd6-fb7faa704f15)

1. 게임의 목표는 골드 카드를 모두 수집하는 것입니다.
2. 수집된 카드와 1 위나 1 아래인 (초록색 보드 위) 카드를 눌러 수집할 수 있습니다.
3. (보드에서 수집할 수 있는 카드가 없다면) 덱을 눌러 새로운 카드를 수집된 카드로 받을 수 있습니다.
4. 덱의 개수가 0 이 되면 레벨 클리어 실패입니다.  


## Unity 에디터에서 플레이 가능한 씬
![image](https://github.com/gemfile0/PyramidSolitaireSagaSample/assets/369285/5723c399-4531-47d0-9d59-0888356132ac)

1. RuntimeLevelEditor 씬 - 레벨 디자인을 수행할 수 있습니다. 레벨 에디터에서 변경한 설정은 즉시 레벨 데이터 파일에 기록합니다.
![image](https://github.com/gemfile0/PyramidSolitaireSagaSample/assets/369285/25f64336-4182-4f2f-84ac-d5244d1aef9e)

2. LevelMap 씬 - 레벨 데이터 파일들을 로드해 페이지 뷰를 생성합니다. 레벨 버튼을 누르면 특정 레벨을 플레이 할 수 있습니다.
![image](https://github.com/gemfile0/PyramidSolitaireSagaSample/assets/369285/15ad0ef3-2df7-40f3-b1eb-cf81d7b71f6d)
