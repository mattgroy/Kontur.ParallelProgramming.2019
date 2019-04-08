import os
import random
import threading

# '''ClusterServer.exe -p 8061 -n qqq -d 3000 -a'''

word = 'qqq'
threads = []

# for e in range(8060, 8080):
for e in (8010, 8020, 8060):
    threads.append(
        threading.Thread(
            target=os.system,
            args=(f'ClusterServer.exe -p {e} -n {word} -d {random.randint(100, 1500)} -a',)
		)
	)

for e in threads:
    e.start()