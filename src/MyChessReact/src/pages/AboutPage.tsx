import React from "react";
import "./AboutPage.css";
import { Link } from "react-router-dom";

export function AboutPage() {
    return (
        <div>
            <div className="title">About</div>
            <div className="aboutText-Container">
                <div className="aboutText">
                    My Chess is social (and not that serious) chess game where
                    you can play chess online with your friends.
                    You can comment your moves and put some pressure to your friends (in fun way of course!).
                    <br />
                    <br />
                </div>
                <hr />
                <br />
                <div className="subtitle">Third-party Licenses</div>
                <div className="aboutText">
                    My Chess uses chess piece pictures from &nbsp;
                    <Link to="http://en.wikipedia.org/wiki/Chess_piece" className="About-link">
                        Wikipedia
                    </Link>.<br />
                    See license details below:<br />
                    <code>
                        Copyright © 2012 Colin M.L. Burnett<br />
                        Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:<br />
                        1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.<br />
                        2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.<br />
                        3. Neither the name of The author nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.<br />
                        <br />
                        THIS SOFTWARE IS PROVIDED BY THE AUTHOR AND CONTRIBUTORS "AS IS AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHOR AND CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
OF SUCH DAMAGE.
                    </code>
                </div>
            </div>
        </div>
    );
}