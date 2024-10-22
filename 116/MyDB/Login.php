<?php
	$u_id = $_POST["Input_id"];
	$u_pw = $_POST["Input_pw"];

	//echo "$u_id<br>";
	//echo "$u_pw<br>";

	$con = mysqli_connect("Localhost", "typhoon", "jun3824!", "typhoon");
	//"Localhost" <-- 같은 서버 내에 있는 DB 를 찾겠다는 의미
	
	if( !$con )
		die( "Could not connect" . mysqli_connect_error() );

	$check = mysqli_query( $con, " SELECT * FROM NumberGame WHERE user='{$u_id}' " );
	
	$numrows = mysqli_num_rows($check);
	if($numrows == 0)
	{
		//mysqli_num_roes() 함수는 데이터베이스에서 쿼리를 보내서 나온 행의 개수를
		//알아낼 때 쓰임 즉 0 이라는 뜻은 해당 조건을 못 찾았다는 뜻	
	
		die("Id does not exist.");
	}

	$row = mysqli_fetch_assoc($check); //user_id 이름에 해당하는 행의 내용을 가져온다.
	if($row)
	{
		if( $u_pw == $row["pass"] )
		{
			
			// PHP에서 JSON 생성 코드
			$ArrData = array();
			$ArrData["nick_name"]  = $row["nick_name"];
			$ArrData["best_score"]  = $row["best_score"];
			$ArrData["my_gold"] = $row["game_gold"];
			$ArrData["info"]  	      = $row["info"];
			$output = json_encode($ArrData, JSON_UNESCAPED_UNICODE);
			
			// JSON으로 변환
			header('Content-Type: application/json');
			echo $output;
			echo "\n";
			echo "Login_Success!!";
		}
		else
		{
			die("Password does not Match.");
		}
	}

	mysqli_close($con);
?>