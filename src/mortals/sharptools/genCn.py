'''
CADIE numbers generator.

Sample usage to generate CADIE number with requested length:
    genCn.py 101    => 101 byte
    genCn.py 12K    => 12 kilobytes
    genCn.py 200M   => 200 megs
    genCn.py 10G    => 10 gigs

Generator uses Champernowne constant for better compression
but adds some "noise" in between iterations.
@see http://en.wikipedia.org/wiki/Champernowne_constant    
'''

#INFO : Dainius has crapped some program to generate CADIE numbers,
# we'll use it this year.

if __name__ == '__main__':
    pass