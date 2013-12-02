<?php
set_time_limit (0);
$transName = 'list_tweets';
$cacheTime = 10;
//echo PHP_INT_MAX;
function textClean($text){
	$trans = array(
		"\n"=>"_NL_"
	);
	return strtr($text, $trans);
}
     // require the twitter auth class
     require_once 'twitteroauth-master/twitteroauth/twitteroauth.php';
     $twitterConnection = new TwitterOAuth(
					'NljcPnGzwLjydNPAMGbKFQ',	// Consumer Key
					'1kSMaEA4zlbpPf0YpLg2iZ2nywBHSBNvl99wpiiC0k',   	// Consumer secret
					'47327973-WwUfHiDaMEdHJyptJ1h2xvnatpjlp5LYoRB9DTZEg',       // Access token
					'pyoNx32dBw9zDoJP13IM7rm6LV25gR4VZvZY3go1mH37e'    	// Access token secret
					);
			
	$starttime = time();
	$emo = '.';
	for($round = 1; $round<=100; $round++){
		$fileIndex = (ceil($round/10));
		echo $fileIndex ;
		$file = fopen("sample_$fileIndex.txt","a");
		for ($page = 1; $page<100; $page++){
			if($page==1 && $round == 1){
			 $twitterData = $twitterConnection->get(
							  'search/tweets',
							  array(
								'q'     => $emo,
								'result_type' => 'recent',
								'lang' => 'en',
								'count' => '100'							
							  )
							);
				}
			else {
				$twitterData = $twitterConnection->get(
							  'search/tweets',
							  array(
								'q'     => $emo,
								'result_type' => 'recent',
								'lang' => 'en',
								'count' => '100',
								'max_id' =>$id
							  )
							);
				}
			$statuses = $twitterData ->{'statuses'};
			$i=1;
			echo "\n<h3>$round/$page</h3>\n";
			foreach($statuses as $tweet)
			{
				//print_r($tweet);
				$text = $tweet->{'text'};
				$id = $tweet->{'id_str'};
				$rt = isset($tweet->{'retweeted_status'});
				$text = textClean($text);
				//print($text);
				//echo "-- $i -- $id <br/>";
				if($i!=1 && $rt==false && strlen($text)>50)fprintf($file,"%s\n",$text);
				$i++;
			}
			echo "time passed ".((time()-$starttime)/60)." min";
			ob_flush();
			flush();
			sleep(6);
		}
		//print( "<b>waiting 15 minutes</b>");
		fclose($file);
		sleep(10);
	}
?>