> 안녕하세요. 게임 클라이언트 개발자로 입사 지원한 박기용 입니다. 이 프로젝트는 함께 퇴직한 이전 직장 동료분들과 함께 출시를 목표로, 9월 현재까지 작업 진행중인 토이 프로젝트의 (5월까지 작업했던) 프로토타입 입니다. 다른 제작자 분들이 작업에 참여하기 전까지의 직접 작성한 더미 그래픽으로만 구현되어 있습니다. 프로토타입에는 포함하지 못했으나, 메인 게임인 퍼즐 게임은 TriPeaks 솔리테어 게임으로 진행하고, 이를 통해 모은 재화로 로비에서 서브로 스토리 게임을 진행하는 구조입니다.
>
> 9월 현재는 퍼즐 게임에 필요한 요소들의 클라이언트 기능을 대부분 구현한 상태이며 최적화 작업을 진행중입니다. 현재 계획은 게임 재화와 관련된 데이터들은 모두 Unity Cloud 를 이용해 저장하려고 합니다.

# Pyramid Solitaire Saga Sample
King.com 의 TriPeaks 솔리테어 게임인 Pyramid Solitaire Saga 를 따라 만든 샘플 Unity 프로젝트입니다.

- [WebGL 빌드 플레이 링크 (itch.io)](https://gemfile0.itch.io/pyramid-solitaire-saga-made-with-unity?secret=CfcK7JLitcgcBErwJd38efX57gY)
- [11레벨 디자인 후 직접 플레이하는 영상 링크 (YouTube)](https://youtu.be/FUR7Q9k_uoo)  
- [50레벨 디자인 후 직접 플레이하는 영상 링크 (YouTube)](https://youtu.be/xlwPmFhutOU)
  > [레퍼런스 게임 플레이 영상 링크 (YouTube)](https://youtu.be/YH51ldCczJ8)


## 사용한 Unity 에디터 버전
Unity 에디터 버전 2022.3.21f1 을 사용했습니다.


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
