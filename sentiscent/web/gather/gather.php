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
	
	$file = fopen("joys.txt","a");		
	
	$emo = ':D';
	
	for ($page = 0; $page<111; $page++){
		if($page==0){
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
		echo "\n<h1>$page</h1>\n";
		foreach($statuses as $tweet)
		{
			 $text = $tweet->{'text'};
			 $id = $tweet->{'id_str'};
			 $text = textClean($text);
			 print($text);
			 echo "-- $i -- $id <br/>";
			 if($i!=100)fprintf($file,"%s\n",$text);
			 $i++;
		}
	}
?>