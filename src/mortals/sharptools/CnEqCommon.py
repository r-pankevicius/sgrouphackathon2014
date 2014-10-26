'''
Common stuff for CADIE numbers comparers.
'''

import sys

class RanAndStuckError(RuntimeError):
    '''Runtime exception with error code'''
    
    def __init__(self, errorCode, message):
        RuntimeError.__init__(self, message)
        self.errorCode = errorCode


# -----------
def usage():
    print('CADIE numbers comparer.')
    print('Usage:')
    print('  %s file1 file2' % sys.argv[0])


# -----------
def toCompareSign(resultNumber):
    if resultNumber == 0:
        return '='
    elif resultNumber > 0:
        return '>'
    else:
        return '<'

# -----------
def toExitCode(resultNumber):
    if resultNumber == 0:
        return 0
    elif resultNumber > 0:
        return 1
    else:
        return 2

# -----------
# Self-checks
#
def iWishIWasntHere():
    # HTTP 403 Forbidden
    raise RanAndStuckError(403, 'PROGRAM HAS GOTTEN LOST.')

def assertEqual(x, y):
    if x <> y:
        # HTTP 412 Precondition Failed
        raise RanAndStuckError(412, 'PROGRAM IS TOO BADLY BROKEN TO RUN.')

