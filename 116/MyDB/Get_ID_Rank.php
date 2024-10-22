<?php
	
	// < 유료리눅스버전 >
	// PHP 버전 PHP 7.3 : ANSI 로 저장해야 한글이 안깨진다.
	// DB 정보  MySQL 5.7 : mysqli_query($con, "SET @curRank := 0"); 
	// 사용 가능 @curRank := @curRank + 1 as myrankidx

	// < 무료버전 >
	// PHP 버전 PHP 7.4 : 한글이 UTF-8 로 저장해야 한글이 안깨진다.
	// DB 정보  MySQL 8.0 : 무료 SQL 버전에서 rank 는 예약어이다. 그래서 --> myrankidx 바꿨다.
	// 내 순위를 받아올 때 rank() over 함수를 사용해야 정상적으로 내 랭킹을 받아온다.

	$u_id = $_POST["Input_user"];

	if( empty($u_id) )
		die("u_id is empty.");

	$con = mysqli_connect("localhost", "typhoon", "jun3824!", "typhoon");
	//"localhost" <-- 같은 서버 내

	if(!$con)
		die("Could not Connet" . mysqli_connect_error());
	//연결 실패 했을 경우 이 스크립트를 닫아주겠다는 뜻

	$check = mysqli_query($con, "SELECT user FROM NumberGame WHERE user = '{$u_id}' ");

	$numrows = mysqli_num_rows($check);
	if(!$check || $numrows == 0)
	{   
		die("ID does not exist.");
	}

	$JSONBuff = array();

	//-------- 10위 안 리스트 JSON형식으로 만들기...
	$sqlList = mysqli_query($con, "SELECT * FROM NumberGame ORDER BY best_score DESC LIMIT 0, 10");
	// 0에서부터 10명까지만...
	// * <-- 해당 행의 모든 컬럼(COLUMN)을 가져오라는 뜻
	// (특정 컴럼(COLUMN)들만 선택적으로 자져올 수 있다. 쉼표로 구분해사...)
	// 정렬옵션 
	// 오름차순(ASC)  : 작은값부터 큰값 순서 ex) 1, 2, 3, 4...
	// 내림차순(DESC) : 큰값에부터 작은값 순서 ex) 5, 4, 3, 2, 1 ...

	$rowsCount = mysqli_num_rows($sqlList);
	if(!$sqlList || $rowsCount == 0)
	{
		die("List does not exist.");
	}

	$RowDatas = array();
	$Return   = array();

	for($ii = 0; $ii < $rowsCount; $ii++)
	{
		$a_row = mysqli_fetch_array($sqlList);	 //행 정보 하나를 가져오기
		if($a_row != false)
		{
			//JSON 생성을 위한 변수
			$RowDatas["user_id"]	= $a_row["user"];
			$RowDatas["nick_name"]	= $a_row["nick_name"];
			$RowDatas["best_score"] 	= $a_row["best_score"];
			array_push($Return, $RowDatas);
			//JSON 데이터 생성을 위한 배열에 레코드 값 추가
		}//if($a_row != false)
	}//for($ii = 0; $ii < $rowsCount; $ii++)

	$JSONBuff["RkList"] = $Return;	//배열 이름에 배열 넣기
	//-------- 10위 안 리스트 JSON형식으로 만들기..

	//-------- 자신의 랭킹 순위 찾아오기...
	//그룹화하여 데이터 조회 (GROUP BY) https://extbrain.tistory.com/56

	//http://cremazer.blogspot.com/2013/09/mysql-rownum.html : 참고함(1번방법)
	//https://wedul.site/434  //https://link2me.tistory.com/536  //https://lightblog.tistory.com/190 //<--장단점
	//SQL에서 변수는 앞에 @을 붙인다.
	//변수는 앞에 @을 붙인다.
	//변수에 값을 할당시 set, select로 할 수 있다. 할당시에는 := 로 한다. 
//	mysqli_query($con, "SET @curRank := 0"); //(MY SQL 내에서 사용할 변수 선언법) 변수 사용은 새션내에서만 유효합니다.  MySQL 버전: 5.7.35
//	$check = mysqli_query($con, "SELECT user_id, myrankidx 
//          	      		FROM (SELECT user, 
//              	    		@curRank := @curRank + 1 as myrankidx 
//              			FROM NumberGame
//				ORDER BY best_score DESC) as CNT 
//	  	      	WHERE `user`='{$u_id}' ");

	//무료버전 MySQL 8.0 버전만 지원
	$check = mysqli_query($con, "SELECT user, myrankidx 
				FROM (SELECT user, 
				rank() over(ORDER BY best_score DESC) as myrankidx 
			FROM NumberGame) as CNT 
			WHERE user='{$u_id}' ");

	//as 는 변수의 별칭을 만들어서 사용하겠다는 뜻(형변환과 비슷)  			
	// https://recoveryman.tistory.com/172

	$numrows = mysqli_num_rows($check);
	if(!$check || $numrows == 0)
	{
		die("Ranking search failed for ID.");
	}

	if($row = mysqli_fetch_assoc($check)) //찾은 user_id 이름에 해당하는 행을 하나만
	{// 가져오기 < 연관 배열 가져오기 : 키값 배열 >
		//JSON 파일 생성
		$JSONBuff["my_rank"] = $row["myrankidx"];
		$output = json_encode($JSONBuff, JSON_UNESCAPED_UNICODE);//한글 포함된 경우
		echo $output;
		echo ("\n");
		echo "Get_Rank_List_Success~";
	}
	//-------- 자신의 랭킹 순위 찾아오기...

	mysqli_close($con);
?>