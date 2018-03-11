#!/usr/bin/env python3

#in this version we remake the lexer to character-by-character

import re
import sys
import math

class ScratchLexer():
    def __init__(self, text):
        self.position = 0
        self.text = text

    def nextWord(self):
        if self.position >= len(self.text):
            return None
        while self.text[self.position].isspace():
            self.position += 1
            if self.position >= len(self.text):
                return None
        new_pos = self.position
        while not self.text[new_pos].isspace():
            new_pos += 1
            if new_pos >= len(self.text):
                break
        collector = self.text[self.position:new_pos]
        self.position = new_pos + 1
        return collector

    def nextCharsUpTo(self,c):
        if self.position >= len(self.text):
            return None
        new_pos = self.position
        while self.text[new_pos] != c:
            new_pos += 1
            if new_pos > len(self.text):
                raise Exception("Unexpected end of input")
        collector = self.text[self.position:new_pos]
        self.position = new_pos + 1
        return collector

        #if self.next >= len(self.words):
        #    return None
        #else:
        #    ret = self.words[self.next]
        #    self.next += 1
        #    return ret

#our type that represents a variable
class VarObj():
    def __init__(self, val):
        self.val = val
    def __repr__(self):
        return "VarObj:" + str(self.val)
    def __str__(self):
        return str(self.val)
    def __add__(self,other):
        return self.val + other
    def __sub__(self,other):
        return self.val - other
    def __mul__(self,other):
        return self.val * other
    def __div__(self,other):
        return self.val/other
    def __radd__(self,other):
        return self.val + other
    def __rsub__(self,other):
        return other - self.val
    def __rmul__(self,other):
        return self.val*other
    def __rdiv__(self,other):
        return other/self.val

#==== Language Commands ====

def PRINT(scratch):
    print(scratch.stack.pop())

def PSTACK(scratch):
    print(scratch.stack)

def stackcheck(scratch,n):
    if (len(scratch.stack) < n):
        raise Exception("Not enough items on stack")

#unary operator
def UNOP(scratch,f):
    if (len(scratch.stack) < 1):
        raise Exception("Not enough items on stack")
    scratch.stack.append(f(scratch.stack.pop()))

#like UNOP but with no stack appendig
def UOP(scratch,f):
    if (len(scratch.stack) < 1):
        raise Exception("Not enough items on stack")
    f(scratch.stack.pop())


#binary operator
def BINOP(scratch, f):
    if (len(scratch.stack)<2):
        raise Exception("Not enough items on stack")
    t = scratch.stack.pop()
    scratch.stack.append(f(t, scratch.stack.pop()))

#like BINOP but with no stack appending
def BOP(scratch, f):
    if (len(scratch.stack)<2):
        raise Exception("Not enough items on stack")
    t = scratch.stack.pop()
    f(t,scratch.stack.pop())


def ADD(scratch):
    BINOP(scratch, lambda x,y:x+y)

def SUB(scratch):
    BINOP(scratch, lambda x,y:x-y)

def MULT(scratch):
    BINOP(scratch, lambda x,y:x*y)

def DIV(scratch):
    BINOP(scratch, lambda x,y:x/y)

def SQRT(scratch):
    UNOP(scratch, lambda x:math.sqrt(x))

def MKVAR(scratch):
    var_name = scratch.lexer.nextWord()
    if (var_name == None):
        raise Exception("Unexpected end of Input")
    scratch.define(var_name, VarObj(0)) #,makeVariable(scratch))

def CONST(scratch):  #let's call this a pseudo-constant for now
    var_name = scratch.lexer.nextWord()
    if (var_name == None):
        raise Exception("Unexpected end of Input")
    UOP(scratch, lambda x: scratch.define(var_name, x)) #,makeVariable(scratch))
    

def storeimpl(varref,newval):
    varref.val = newval
    return varref

def STORE(scratch):
    BOP(scratch,storeimpl)

#TODO, Make STORE and FETCH lookahead maybe
     # Also clean up and make things nicer I guess

#Replaces reference to variable on TOS with its value
def FETCH(scratch):
    UNOP(scratch, lambda x : x.val)

def STRNG(scratch):
    scratch.stack.append(scratch.lexer.nextCharsUpTo('\"'))

#the same thing as above but we ignore the contents
def COMMENT(scratch):
    next_word = scratch.lexer.nextWord()
    while next_word[-2:] != '*/':
        next_word = scratch.lexer.nextWord()
        if next_word is None:
            raise Exception("Unexpected end of input")

def DUP(scratch):
    stackcheck(scratch,1)
    tos = scratch.stack.pop()
    scratch.stack.append(tos)
    scratch.stack.append(tos)

def DROP(scratch):
    stackcheck(scratch,1)
    tos = scratch.stack.pop()

def SWAP(scratch):
    stackcheck(scratch,2)
    tos = scratch.stack.pop()
    _2os = scratch.stack.pop()
    scratch.stack.append(tos)
    scratch.stack.append(_2os)

def OVER(scratch):
    stackcheck(scratch,2)
    tos = scratch.stack.pop()
    _2os = scratch.stack.pop()
    scratch.stack.append(_2os)
    scratch.stack.append(tos)
    scratch.stack.append(_2os)

def ROT(scratch):
    stackcheck(scratch,3)
    tos = scratch.stack.pop()
    _2os = scratch.stack.pop()
    _3os = scratch.stack.pop()
    scratch.stack.append(_2os)
    scratch.stack.append(tos)
    scratch.stack.append(_3os)

class Scratch():
    def __init__(self):
        self.vars={}
        self.dictionary={
                'PRINT' : PRINT,
                'PSTACK': PSTACK,
                'SQRT' : SQRT,
                'VAR' : MKVAR,
                'STORE' : STORE,
                'FETCH' : FETCH,
                'CONST' : CONST,
                'DUP' : DUP,
                'DROP' : DROP,
                'SWAP' : SWAP,
                'OVER' : OVER,
                'ROT' : ROT,
                '+' : ADD,
                '-' : SUB,
                '/' : DIV,
                '*' : MULT,
                '\"' : STRNG,
                '/*' : COMMENT}
        self.stack = []
        self.lexer = None #Make lexer a member because w/ VAR stmt we have to look ahead

    def addWords(self, d):
        for word in d:
            self.dictionary[word.upper()] = d[word]
    #make a var
    def define(self,word,obj):
        self.vars[word.upper()] = obj
        self.dictionary[word.upper()] = lambda s : s.stack.append(s.vars[word.upper()])
    
    def run(self, text):
        self.lexer   = ScratchLexer(text)
        num_val = 0
        word = self.lexer.nextWord()

        while (word != None):
            word = word.upper()
            num_val = None
            try:
                num_val = float(word)
            except:
                pass

            if (word in self.dictionary):
                self.dictionary[word](self)  #somehow act on this
            elif num_val != None:
                self.stack.append(num_val)
            else:
                raise Exception("Unknown Word")
            #continue parsing
            word = self.lexer.nextWord()

if __name__ == "__main__":
    if (len(sys.argv) < 2):
        print("Usage: {} <string>".format(sys.argv[0]))
        sys.exit(0)

    terp = Scratch()
    terp.run(sys.argv[1])
