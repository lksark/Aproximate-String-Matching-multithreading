# Aproximate-String-Matching-multithreading
Search Approximate String Matching using multithreading

Search does string 'T' approximately contains string 'P' using multithreading programming.

When string ‘T’ is relatively long compare to string ‘P’:
1.     Check number processing threads available on the current computer.
2.     Split string ‘T’ into smaller segments, number equals to number of system threads.
3.     Each thread runs ‘Approximate Contains’ search function on its given string ‘T’ segments

Afterward, print the results, including location in string 'T' and the differences against string 'P'.
