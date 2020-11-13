if redis.call("EXISTS", @serverKey) == 0 then
    return false
end
local code = redis.call('HGET', @serverKey, "Code")
redis.call("DEL", @serverKey);
redis.call("HDEL", @serversByCodeKey, code)
redis.call("ZREM", @publicServersByPlayerCountKey, @secret)
return true
