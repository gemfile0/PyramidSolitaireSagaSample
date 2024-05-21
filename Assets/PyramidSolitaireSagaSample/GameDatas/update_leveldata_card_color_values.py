import os

# 현재 디렉토리의 모든 .asset 파일 검색
for filename in os.listdir('.'):
    if filename.endswith('.asset'):
        # 파일을 읽기 모드로 열기
        with open(filename, 'r', encoding='utf-8') as file:
            content = file.read()
        
        # 첫 번째 변경: 1을 2로 변경
        content = content.replace('<CardColor>k__BackingField":1', '<CardColor>k__BackingField":2')
        
        # 두 번째 변경: 0을 1로 변경
        updated_content = content.replace('<CardColor>k__BackingField":0', '<CardColor>k__BackingField":1')
        
        # 변경된 내용을 파일에 다시 쓰기
        with open(filename, 'w', encoding='utf-8') as file:
            file.write(updated_content)

print("모든 .asset 파일의 내용이 업데이트 되었습니다.")
