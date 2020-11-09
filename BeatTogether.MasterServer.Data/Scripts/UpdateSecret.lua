if redis.call("EXISTS", @serverKey) == 1 then
    redis.call("HSET", @serverKey, "Secret", @secret)
end
