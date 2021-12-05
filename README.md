# Batch

Sample app to process incomming requests in background with batching or timeout

## How it works

Collection phase

```
User -> GET /collect?id=1 -> processor.enqueue(payload)
```

Processing phase

```
while app.isNotClosing:
    batch = []
    timer = 5s

    while batch.isNotFull and timer.isNotFired:
        batch += queue.dequeue
    
    process(batch)
```