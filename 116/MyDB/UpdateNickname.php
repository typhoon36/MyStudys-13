<?php
	$u_id = $_POST["Input_user"];
	$nick = $_POST["Input_nick"];

	if( empty( $u_id) )
		die("Input_user is empty.");
	if( empty($nick) )
		die("Input_nick is empty.");

	$con = mysqli_connect("Localhost", "typhoon", "jun3824!", "typhoon");
	//"Localhost" <-- 같은 서버 내에 있는 DB를 찾겠다는 의미

	if( ! $con )
		die( "Could not connect" . mysqli_connect_error() );

	$check = mysqli_query( $con, "SELECT user FROM NumberGame WHERE user= '" . $u_id ."'" );

	$numrows = mysqli_num_rows($check);
	if($numrows == 0)
	{
		//mysqli_num_rows() 함수는 데이터베이스에서 쿼리를 보내서 나온 행의 개수를
		//알아낼 때 쓰임 즉 0 이라는 뜻은 해당 조건을 못 찾았다는 뜻
		die( "Id does not exist." );
	}


	$check = mysqli_query( $con, "SELECT nick_name FROM NumberGame WHERE nick_name= '{$nick}' " );

	$numrows = mysqli_num_rows($check);
	if($numrows != 0)
	{
		die("Nickname does exist.");
	}

	$Result = mysqli_query($con, "UPDATE NumberGame SET nick_name = '{$nick}' WHERE user = '{$u_id}' ");
	// user_id 를 찾아서 nick_name = '$nick' 로 변경해 줘 라는 뜻

	if($Result)
		echo "Update Nick Success.";
	else
		echo "Update Nick error.";

	mysqli_close($con);
?> 