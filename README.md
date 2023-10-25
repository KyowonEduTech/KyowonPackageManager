
<img src="https://capsule-render.vercel.app/api?type=Waving&color=auto&height=300&section=header&text=Kyowon's%20Unity%20Package%20Manager&fontSize=30" />

Kyowon 의 Unity 개발 공통 모듈을 추가/삭제/업데이트 할 수 있는 Kyowon 만의 Unity Package Manger 입니다.

1) 개인 Github 계정을 Project 담당자에게 전달하여 KyowonEduTech Github 접근 권한을 요청하세요.
   
2) Unity Package Manager 로 KyowonPackageManager 를 설치해 주세요.
   https://github.com/KyowonEduTech/KyowonPackageManager.git 로 설치할 수 있습니다.
   
3) 프로젝트 오픈 후, [메뉴] - [Kyowon] - [Open Kyowon Package Manager] 를 클릭하여 GitHub KyowonEduTech Repository 의 권한을 인증 받습니다.
   *Github Access Token(classic) 발급 시, scope 에서 read:packages 를 체크해주어야 합니다.
   
4) 인증 후, 추가로 필요한 선택 모듈을 다운받습니다.
   Install, Remove, Update 기능이 제공되며 Packages 폴더에 설치/삭제 됩니다.

5) 모듈은 KyowonPackageManifest.json 파일과 .gitignore 파일로 관리됩니다.
   모듈 추가, 삭제, 업데이트 시, KyowonPackageManifest.json 와 .gitignore 파일도 함께 생성되며 모듈 내용이 저장됩니다.
   프로젝트를 Git 에 올릴 때, KyowonPackageManifest.json 파일을 꼭 추가하여 Commit 해주세요. 다른 팀원은 해당 파일들만 받으면 해당 모듈 버전으로 자동 설치 및 관리됩니다.
