<?php
$transName = 'list_tweets';
$cacheTime = 10;

     // require the twitter auth class
     require_once 'twitteroauth-master/twitteroauth/twitteroauth.php';
     $twitterConnection = new TwitterOAuth(
					'NljcPnGzwLjydNPAMGbKFQ',	// Consumer Key
					'1kSMaEA4zlbpPf0YpLg2iZ2nywBHSBNvl99wpiiC0k',   	// Consumer secret
					'47327973-WwUfHiDaMEdHJyptJ1h2xvnatpjlp5LYoRB9DTZEg',       // Access token
					'pyoNx32dBw9zDoJP13IM7rm6LV25gR4VZvZY3go1mH37e'    	// Access token secret
					);
     $twitterData = $twitterConnection->get(
					  'search/tweets',
					  array(
					    'q'     => ':(',
						'result_type' => 'recent',
						'lang' => 'en',
						'count' => '100'
					  )
					);
	//echo $twitterData;				
	//echo "\n<br/>\n";
	/*	$twitterData = $twitterConnection->get(
	  'statuses/user_timeline',
	  array(
		'screen_name'     => 'nafSadh',
		'count'           => 100,
		'exclude_replies' => false
	  )
	);*/
	//if($twitterData==null) echo 'td null<br/>';
	//print_r ( $statuses );
	//echo $statuses->count;
	//$tweets = json_decode($statuses);
	$statuses = $twitterData ->{'statuses'};
	$i=1;
	foreach($statuses as $tweet)
	//foreach($twitterData as $tweets)
	{
		 $text = $tweet->{'text'};
		 print();
		 echo "<br/>\n";
		 $i++;
	}

?>