import "./MainPage.css"
function MainPage() {
    return (
        <div className="page">
          <div className='backg-img'>

              <div className='wrapper'>
                    <nav>
                        <img className="logo" />
                      <div className='menu-info'>
                          <ul>
                              <li>Explore the GOT world</li>
                              <li>Team</li>
                              <li>
                                  <button className='login-btn'>
                                      Log in
                                  </button>
                              </li>
                          </ul>
                      </div>
                  </nav>
                  <div className='titles'>
                      <div className='title-font'>
                          when you play game of thrones
                      </div>
                      <div className='main-title-font'>
                          you win or you die
                      </div>
                      <button className='start-btn'>START NOW</button>
                  </div>
                    <img className='dragon-img' />
                    <img className='fire-img' />
                    <img className='bg-img' />
                </div>
          </div>
          <hr></hr>
      </div>
  );
}

export default MainPage;