
OPTION CASEMAP:NONE

onePixel macro
;Za�aduj do ymm1 i ymm2 piksele
 vmovdqu ymm1, ymmword ptr[RDI]
 vmovdqu ymm2, ymmword ptr[RDI]
;Rozpakuj high bytes do ymm1 a low do ymm2, YMM11 == 0 co pozwala na zamian� bajt�w na wordy
 vpunpckhbw	ymm1, ymm1, ymm11
 vpunpcklbw	ymm2, ymm2, ymm11
;Przeka� do ymm3 warto�� filtra dla konkretnego piksela
 vpbroadcastw ymm3, word ptr[RBX]
;Przemn� piksel przez filtr
 vpmullw ymm1, ymm1, ymm3
 vpmullw ymm2, ymm2, ymm3
;Zapisz sumy do ymm4 i ymm5
 vpaddw ymm4,ymm4,ymm1
 vpaddw ymm5,ymm5,ymm2
;Przejd� do kolejnego piksela i kolejnej warto�ci filtru
 add rbx, 2
 add rdi, 3
 endm

.code
;ASMFilter(unsigned char outData[] -> RSI, unsigned char data[] -> RDI, int imWidth -> R8, int i -> R9, short int filter[] -> RSP+40)

MyProc2 proc EXPORT
;Ustaw RSI jako wska�nik na dane wyj�ciowe
 mov RSI, RCX
;Ustaw RDI jako wska�nik na dane wej�ciowe
 mov RDI, RDX
;Wyzeruj ymm4 i ymm5
 vpxor ymm4, ymm4, ymm4
 vpxor ymm5, ymm5, ymm5
;Przesu� wska�nik na dane wej��iowe i wyj�ciowe o indeks pikseli
 add rdi, r9
 add rsi, r9
;Za�aduj do RAX d�ugo�� zdj�cia pomno�on� przez 3 (poniewa� jeden piksel sk�ada si� z 3 bajt�w)
 mov rax, 3
 mul r8
;Ustaw rdi na piksel znajduj�cy si� na lewo i do g�ry od piksela wej�ciowego
 sub rdi, rax
 sub rdi, 3
;Za�aduj do rbx pierwsz� liczb� filtra (lewy g�rny r�g)
 mov rbx, qword ptr [rsp+40];
;Wykonaj 3 razy makro obliczaj�ce warto�� piksel*filtr
 onePixel
 onePixel
 onePixel
;Przesu� rdi na pocz�tek kolejnego rz�du
 sub rdi, 9
 add rdi, rax

 onePixel
 onePixel
 onePixel
 
 sub rdi, 9
 add rdi, rax
 onePixel
 onePixel
 onePixel

;Po��cz obliczone w ymm5 i ymm4 warto�ci sum dla kolejnych piksli
 vpackuswb ymm0, ymm5, ymm4
;Zapisz piksel do tablicy wyj�ciowej
 vmovdqu ymmword ptr[RSI+3], ymm0
 
 ret ; return z in EAX register
MyProc2 endp
end ; End of ASM file