class RanAndStuckError(RuntimeError):
    '''Runtime exception with error code'''
    
    def __init__(self, errorCode, message):
        RuntimeError.__init__(self, message)
        self.errorCode = errorCode

