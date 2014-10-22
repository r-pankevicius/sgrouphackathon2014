'''
CADIE numbers comparer.

Usage:
    cneq.py file1 file2
Files are equal if errorlevel=0.
'''

import sys
import os.path

# -----------
def main(argv):

    if len(argv) <> 2:
        usage()
        sys.exit(400) # HTTP 400 Bad Request

    fileName0 = argv[0]
    assureFileExistsAndNotEmpty(fileName0, 0)

    fileName1 = argv[1]
    assureFileExistsAndNotEmpty(fileName1, 1)

    with open(fileName0, 'rb') as f0:
        with open(fileName1, 'rb') as f1:
            result = compareFiles(f0, f1)
            sys.exit(result)

# -----------
def compareFiles(f0, f1):

    sgn0 = readSignAndFirstDigit(f0)
    sgn1 = readSignAndFirstDigit(f1)

    #TODO: type something more here


# -----------
"""
Reads a sign::=oneof(-1,+1) and first digit::=number[0..9] in file f.
Returns [sign, first digit]
"""
def readSignAndFirstDigit(f):
    byte = f.read(1)
    if byte == '-' or byte == '+':
        pass
        #return [byte == '-' ? -1 : +1, None]
    #TODO: type something more here

# -----------
def assureFileExistsAndNotEmpty(fileName, fileIdx):

    if not os.path.isfile(fileName):
        print('File "%s" doesn\'t exist.' % fileName)
        sys.exit(404 + fileIdx) # HTTP 404 Not Found, 405 Method Not Allowed

    # Empty file is invalid
    fileSize = os.path.getsize(fileName)
    if fileSize == 0:
        print('File "%s" is empty.' % fileName)
        sys.exit(204 + fileIdx) # HTTP 204 No Content, 205 Reset Content


# -----------
def usage():
    print('CADIE numbers comparer.')
    print('Usage:')
    print('  %s file1 file2' % sys.argv[0])
    print('Files are equal if errorlevel=0.')

# -----------
if __name__ == '__main__':
    main(sys.argv[1:])
